export interface ExampleMockProductCategoryModel {
	id: string;
	name: string;
}

export const productCategories: Array<ExampleMockProductCategoryModel> = [
	{ id: 'cat-hoodie', name: 'Hoodies' },
	{ id: 'cat-jacket', name: 'Jackets' },
	{ id: 'cat-shorts', name: 'Shorts' },
	{ id: 'cat-trousers', name: 'Trousers' },
	{ id: 'cat-tshirt', name: 'T-shirts' },
];
