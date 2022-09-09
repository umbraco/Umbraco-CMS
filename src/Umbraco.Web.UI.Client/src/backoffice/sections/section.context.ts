import { BehaviorSubject, ReplaySubject } from 'rxjs';

import type { ManifestSection, ManifestTree } from '../../core/models';
import { Entity } from '../../mocks/data/entities';

export class UmbSectionContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<ManifestSection>({
		type: 'section',
		alias: '',
		name: '',
		js: '',
		elementName: '',
		meta: {
			pathname: '',
			weight: 0,
		},
	});
	public readonly data = this._data.asObservable();

	// TODO: what is the best context to put this in?
	private _activeTree = new ReplaySubject<ManifestTree>(1);
	public readonly activeTree = this._activeTree.asObservable();

	// TODO: what is the best context to put this in?
	private _activeTreeItem = new ReplaySubject<Entity>(1);
	public readonly activeTreeItem = this._activeTreeItem.asObservable();

	constructor(section: ManifestSection) {
		if (!section) return;
		this._data.next(section);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<ManifestSection>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}

	public setActiveTree(tree: ManifestTree) {
		this._activeTree.next(tree);
	}

	public setActiveTreeItem(treeItem: Entity) {
		this._activeTreeItem.next(treeItem);
	}
}
