export interface ExampleColorModel {
	name: string;
	hex: string;
}

export const exampleColors: Record<string, ExampleColorModel> = {
	'color-black': { name: 'Black', hex: '#1b1b1b' },
	'color-blue': { name: 'Blue', hex: '#2f6fd6' },
	'color-green': { name: 'Green', hex: '#2e8b57' },
	'color-red': { name: 'Red', hex: '#d63a2f' },
	'color-white': { name: 'White', hex: '#ffffff' },
};
