// TODO: This does not belong here, lets move it to the icon package.

export const umbracoColors = [
	{ name: 'Black', alias: 'text', varName: '--uui-color-text' },
	{ name: 'Yellow', alias: 'yellow', varName: '--uui-palette-sunglow' },
	{ name: 'Pink', alias: 'pink', varName: '--uui-palette-spanish-pink' },
	{ name: 'Blue', alias: 'blue', varName: '--uui-palette-violet-blue' },
	{ name: 'Light Blue', alias: 'light-blue', varName: '--uui-palette-malibu' },
	{ name: 'Red', alias: 'red', varName: '--uui-palette-maroon-flush' },
	{ name: 'Green', alias: 'green', varName: '--uui-palette-jungle-green' },
	{ name: 'Brown', alias: 'brown', varName: '--uui-palette-chamoisee' },
	{ name: 'Grey', alias: 'grey', varName: '--uui-palette-dusty-grey' },

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
