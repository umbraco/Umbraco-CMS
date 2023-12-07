const umbracoColors = [
	{ alias: 'text', varName: '--uui-color-text' },
	{ alias: 'yellow', varName: '--uui-palette-sunglow' },
	{ alias: 'pink', varName: '--uui-palette-spanish-pink' },
	{ alias: 'dark', varName: '--uui-palette-gunmetal' },
	{ alias: 'darkblue', varName: '--uui-palette-space-cadet' },
	{ alias: 'blue', varName: '--uui-palette-violet-blue' },
	{ alias: 'red', varName: '--uui-palette-maroon-flush' },
	{ alias: 'green', varName: '--uui-palette-jungle-green' },
	{ alias: 'brown', varName: '--uui-palette-chamoisee' },
];

export function extractUmbColorVariable(colorAlias: string): string | undefined {
	const found = umbracoColors.find((umbColor) => umbColor.alias === colorAlias);
	return found?.varName;
}
