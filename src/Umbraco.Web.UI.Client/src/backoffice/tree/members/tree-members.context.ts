import { ITreeService, UmbTreeItem } from '../tree.service';

export class UmbTreeMemberContext implements ITreeService {
	public async getTreeItem(id: string) {
		return {
			id: 1,
			key: '81ea7423-985a-43d7-b36e-32a128143c40',
			name: 'Hej Jesper',
			hasChildren: true,
		};
	}

	public async getChildren(id: string) {
		return [
			{
				id: 2,
				key: 'f44101ff-8530-4c5d-926e-f2dbdc7b1e4b',
				name: 'Hej Jesper',
				hasChildren: true,
			},
		];
	}
}
