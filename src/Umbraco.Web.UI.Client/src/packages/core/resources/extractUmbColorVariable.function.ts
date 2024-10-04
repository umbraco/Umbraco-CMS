export const umbracoColors = [
	{ alias: 'text', varName: '--uui-color-text' },
	{ alias: 'yellow', varName: '--uui-palette-sunglow' },
	{ alias: 'pink', varName: '--uui-palette-spanish-pink' },
	{ alias: 'blue', varName: '--uui-palette-violet-blue' },
	{ alias: 'light-blue', varName: '--uui-palette-malibu' },
	{ alias: 'red', varName: '--uui-palette-maroon-flush' },
	{ alias: 'green', varName: '--uui-palette-jungle-green' },
	{ alias: 'brown', varName: '--uui-palette-chamoisee' },
	{ alias: 'grey', varName: '--uui-palette-dusty-grey' },

	{ alias: 'black', legacy: true, varName: '--uui-color-text' },
	{ alias: 'blue-grey', legacy: true, varName: '--uui-palette-dusty-grey' },
	{ alias: 'indigo', legacy: true, varName: '--uui-palette-malibu' },
	{ alias: 'purple', legacy: true, varName: '--uui-palette-space-cadet' },
	{ alias: 'deep-purple', legacy: true, varName: '--uui-palette-space-cadet' },
	{ alias: 'cyan', legacy: true, varName: '-uui-palette-jungle-green' },
	{ alias: 'light-green', legacy: true, varName: '-uui-palette-jungle-green' },
	{ alias: 'lime', legacy: true, varName: '-uui-palette-jungle-green' },
	{ alias: 'amber', legacy: true, varName: '--uui-palette-chamoisee' },
	{ alias: 'orange', legacy: true, varName: '--uui-palette-chamoisee' },
	{ alias: 'deep-orange', legacy: true, varName: '--uui-palette-cocoa-brown' },
];

/**
 *
 * @param colorAlias
 */
export function extractUmbColorVariable(colorAlias: string): string | undefined {
	const found = umbracoColors.find((umbColor) => umbColor.alias === colorAlias);
	return found?.varName;
}
