import { ITreeService } from '../tree.service';

export class UmbTreeMemberContext implements ITreeService {
	public async getRoot() {
		return {
			id: -1,
			key: '81ea7423-985a-43d7-b36e-32a128143c40',
			name: 'Hej Jesper',
			hasChildren: true,
		};
	}

	public async getChildren(id: string) {
		// TODO: figure out url structure
		const res = await fetch(`/umbraco/backoffice/trees/members/${id}`);
		const json = await res.json();
		return json;
	}
}
