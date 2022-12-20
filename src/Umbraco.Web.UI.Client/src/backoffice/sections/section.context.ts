import { BehaviorSubject, ReplaySubject } from 'rxjs';
import type { Entity, ManifestSection, ManifestSectionView, ManifestTree } from '@umbraco-cms/models';

export class UmbSectionContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<ManifestSection>({
		type: 'section',
		alias: '',
		name: '',
		js: '',
		elementName: '',
		weight: 0,
		meta: {
			label: '',
			pathname: '',
		},
	});
	public readonly data = this._data.asObservable();

	// TODO: what is the best context to put this in?
	private _activeTree = new ReplaySubject<ManifestTree>(1);
	public readonly activeTree = this._activeTree.asObservable();

	// TODO: what is the best context to put this in?
	private _activeTreeItem = new ReplaySubject<Entity>(1);
	public readonly activeTreeItem = this._activeTreeItem.asObservable();

	// TODO: what is the best context to put this in?
	private _activeView = new ReplaySubject<ManifestSectionView>(1);
	public readonly activeView = this._activeView.asObservable();

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

	public setActiveView(view: ManifestSectionView) {
		this._activeView.next(view);
	}
}
