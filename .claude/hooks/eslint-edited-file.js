// PostToolUse hook: lint backoffice client files right after Claude edits them,
// so lint/Prettier problems are fixed in the same turn instead of in review.
const { spawnSync } = require('child_process');
const path = require('path');

let raw = '';
process.stdin.on('data', (chunk) => (raw += chunk)).on('end', () => {
	const file = (JSON.parse(raw).tool_input || {}).file_path || '';
	const client = path.join(process.env.CLAUDE_PROJECT_DIR, 'src', 'Umbraco.Web.UI.Client');
	const rel = path.relative(client, file);

	if (rel.startsWith('..') || !rel.startsWith('src' + path.sep) || !/\.(ts|js)$/.test(rel)) return;

	const result = spawnSync('npx', ['eslint', rel], {
		cwd: client,
		encoding: 'utf8',
		shell: process.platform === 'win32',
	});
	const output = ((result.stdout || '') + (result.stderr || '')).trim();

	if (output) {
		console.error(`ESLint reported issues in ${rel}. Fix any that your change introduced:\n${output}`);
		process.exit(2);
	}
});
