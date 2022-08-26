import { UmbExtensionManifestTree } from '../../../core/extension';
import { ITreeContext } from '../tree.context';

export class UmbTreeMemberContext implements ITreeContext {
	public tree: UmbExtensionManifestTree;

	constructor(tree: UmbExtensionManifestTree) {
		this.tree = tree;
	}

	public async getRoot() {
		return {
			id: -1,
			key: '81ea7423-985a-43d7-b36e-32a128143c40',
			name: 'Members',
			hasChildren: true,
		};
	}

	public async getChildren(id: number) {
		// TODO: figure out url structure
		const res = await fetch(`/umbraco/backoffice/trees/members/${id}`);
		const json = await res.json();
		return json;
	}
}
