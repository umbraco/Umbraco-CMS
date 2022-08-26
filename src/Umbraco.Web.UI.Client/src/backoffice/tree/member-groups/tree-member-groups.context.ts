import { UmbExtensionManifestTree } from '../../../core/extension';
import { ITreeContext } from '../tree.context';

export class UmbTreeMemberGroupsContext implements ITreeContext {
	public tree: UmbExtensionManifestTree;

	constructor(tree: UmbExtensionManifestTree) {
		this.tree = tree;
	}

	public async getRoot() {
		return {
			id: -1,
			key: 'd46d144e-33d8-41e3-bf7a-545287e16e3c',
			name: 'Member Groups',
			hasChildren: true,
		};
	}

	public async getChildren(id: string) {
		// TODO: figure out url structure
		const res = await fetch(`/umbraco/backoffice/trees/member-groups/${id}`);
		const json = await res.json();
		return json;
	}
}
