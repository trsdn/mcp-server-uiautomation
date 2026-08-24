#!/usr/bin/env node
'use strict';

// Launcher for the bundled UI Automation MCP server.
//
// Nothing may ever be written to stdout here: stdout is the MCP JSON-RPC
// channel and any stray byte corrupts the stream. Diagnostics go to stderr.

const { spawn } = require('node:child_process');
const path = require('node:path');
const fs = require('node:fs');

const exePath = path.join(__dirname, '..', 'server', 'uia-mcp-server.exe');

function fail(message) {
  process.stderr.write(`uia-mcp: ${message}\n`);
  process.exit(1);
}

if (process.platform !== 'win32') {
  fail(
    'this package only runs on Windows. It talks to the Windows UI Automation ' +
      'COM API, which has no equivalent on other platforms.'
  );
}

if (!fs.existsSync(exePath)) {
  fail(
    `the bundled server executable is missing at ${exePath}. ` +
      'Reinstall the package, or build it with npm/Build-NpmPackage.ps1 when working from a clone.'
  );
}

const child = spawn(exePath, process.argv.slice(2), {
  stdio: 'inherit',
  windowsHide: true
});

child.on('error', (error) => {
  fail(`failed to start the server: ${error.message}`);
});

// Let the server own the console: forward the interactive signals instead of
// dying first and leaving it orphaned.
for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP']) {
  process.on(signal, () => {
    if (!child.killed) {
      child.kill(signal);
    }
  });
}

child.on('exit', (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }
  process.exit(code === null ? 1 : code);
});
