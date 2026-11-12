export interface ExampleMockProductColorModel {
	id: string;
	name: string;
	hex: string;
}

export const productColors: Array<ExampleMockProductColorModel> = [
	{ id: 'color-black', name: 'Black', hex: '#1b1b1b' },
	{ id: 'color-blue', name: 'Blue', hex: '#2f6fd6' },
	{ id: 'color-green', name: 'Green', hex: '#2e8b57' },
	{ id: 'color-red', name: 'Red', hex: '#d63a2f' },
	{ id: 'color-white', name: 'White', hex: '#ffffff' },
];
