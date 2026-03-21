import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

/**
 * UIAutomationMcp VS Code Extension
 *
 * This extension provides MCP server definitions for the UIAutomationMcp MCP server
 * and bundles the UIAutomationMcp CLI for direct desktop inspection workflows.
 *
 * The extension bundles self-contained executables for both the MCP server and CLI -
 * no .NET SDK or runtime installation required.
 *
 * Agent Skills are registered via the chatSkills contribution point in package.json.
 */

export async function activate(context: vscode.ExtensionContext) {
	console.log('UIAutomationMcp extension is now active');

	// Register MCP server definition provider
	context.subscriptions.push(
		vscode.lm.registerMcpServerDefinitionProvider('uia-mcp', {
			provideMcpServerDefinitions: async () => {
				const extensionPath = context.extensionPath;
				const mcpServerPath = path.join(extensionPath, 'bin', 'UIAutomationMcp.McpServer.exe');

				return [
					new vscode.McpStdioServerDefinition(
						'uia-mcp',
						mcpServerPath,
						[],
						{
							// Optional environment variables can be added here if needed
						}
					)
				];
			}
		})
	);

	context.subscriptions.push(
		vscode.commands.registerCommand('uiaMcp.copyCliCommand', async () => {
			const cliPath = path.join(context.extensionPath, 'bin', 'uiamcp.exe');
			await vscode.env.clipboard.writeText(`"${cliPath}" desktop`);
			void vscode.window.showInformationMessage('Copied bundled UIAutomationMcp CLI command to the clipboard.');
		})
	);

	context.subscriptions.push(
		vscode.commands.registerCommand('uiaMcp.revealBundledTools', async () => {
			const binPath = path.join(context.extensionPath, 'bin');
			await fs.promises.mkdir(binPath, { recursive: true });
			await vscode.commands.executeCommand('revealFileInOS', vscode.Uri.file(binPath));
		})
	);

	// Show welcome message on first activation
	const hasShownWelcome = context.globalState.get<boolean>('uiamcp.hasShownWelcome', false);
	if (!hasShownWelcome) {
		showWelcomeMessage();
		void context.globalState.update('uiamcp.hasShownWelcome', true);
	}
}

function showWelcomeMessage() {
	const message = 'UIAutomationMcp extension activated. The bundled UI Automation MCP server and CLI are ready.';
	const learnMore = 'Open Repository';

	vscode.window.showInformationMessage(message, learnMore).then(selection => {
		if (selection === learnMore) {
			void vscode.env.openExternal(vscode.Uri.parse('https://github.com/trsdn/mcp-server-uiautomation'));
		}
	});
}

export function deactivate() {
	console.log('UIAutomationMcp extension is now deactivated');
}
