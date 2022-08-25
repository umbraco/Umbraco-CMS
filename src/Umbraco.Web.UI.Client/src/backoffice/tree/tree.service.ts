// import { BehaviorSubject, Observable } from 'rxjs';

export interface ITreeService {
	getTreeItem(id: string): Promise<UmbTreeItem>;
	getChildren(id: string): Promise<Array<UmbTreeItem>>;
}

export interface UmbTreeItem {
	id: number;
	key: string;
	name: string;
	hasChildren: boolean;
}

export class UmbTreeService implements ITreeService {
	public async getTreeItem(id: string) {
		await new Promise((resolve) => setTimeout(resolve, Math.random() * 2000));
		return fakeApi.getTreeItem(id);
	}

	public async getChildren(id: string) {
		await new Promise((resolve) => setTimeout(resolve, Math.random() * 2000));
		return fakeApi.getTreeChildren(id);
	}
}

// EVERYTHING BELOW IS FAKE MOCK DATA AND WILL BE REMOVED

const fakeApi = {
	//find nested child id of array
	getTreeChildren: (id: string) => {
		const children = recursive(treeData, id).children;
		if (!children) return 'not found';

		return children.map((item: any) => {
			return {
				id: item.id,
				name: item.name,
				hasChildren: item.children.length > 0,
			};
		});
	},

	getTreeItem: (id: string) => {
		const item = recursive(treeData, id);
		if (!item) return 'not found';

		return {
			...item,
			hasChildren: item.children.length > 0,
		};
	},

	getTreeRoot: () => {
		return treeData.map((item) => {
			return {
				id: item.id,
				name: item.name,
				hasChildren: item.children.length > 0,
			};
		});
	},
};

const recursive = (data: any, id: string): any => {
	for (let index = 0; index < data.length; index++) {
		const element = data[index];

		if (element.id === id) {
			return element;
		} else {
			const item = recursive(element.children, id);
			if (item) {
				return item;
			}
		}
	}
};

const treeData = [
	{
		id: '1',
		name: 'content',
		children: [
			{
				id: '1-1',
				name: 'content-1-1',
				children: [
					{
						id: '1-1-1',
						name: 'content-1-1-1',
						children: [],
					},
					{
						id: '1-1-2',
						name: 'content-1-1-2',
						children: [],
					},
				],
			},
		],
	},
	{
		id: '2',
		name: 'DataTypes',
		children: [
			{
				id: '2-1',
				name: 'DataTypes-2-1',
				children: [],
			},
			{
				id: '2-2',
				name: 'DataTypes-2-2',
				children: [
					{
						id: '2-2-1',
						name: 'DataTypes-2-2-1',
						children: [],
					},
				],
			},
		],
	},
	{
		id: '3',
		name: 'Something',
		children: [
			{
				id: '3-1',
				name: 'Something-3-1',
				children: [],
			},
			{
				id: '3-2',
				name: 'Something-3-2',
				children: [],
			},
		],
	},
];
