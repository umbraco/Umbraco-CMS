import { BehaviorSubject } from 'rxjs';
import type { Entity, ManifestSection, ManifestSectionView } from '@umbraco-cms/models';
import { UniqueObjectBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbContextToken } from '@umbraco-cms/context-api';

export type ActiveTreeItemType = Entity | undefined;

export class UmbSectionContext {

	#manifest;
	public readonly manifest;

	/*
	This was not used anywhere
	private _activeTree = new BehaviorSubject<ManifestTree | undefined>(undefined);
	public readonly activeTree = this._activeTree.asObservable();
	*/

	// TODO: what is the best context to put this in?
	#activeTreeItem = new UniqueObjectBehaviorSubject<ActiveTreeItemType>(undefined);
	public readonly activeTreeItem = this.#activeTreeItem.asObservable();

	// TODO: what is the best context to put this in?
	#activeViewPathname = new BehaviorSubject<string | undefined>(undefined);
	public readonly activeViewPathname = this.#activeViewPathname.asObservable();

	constructor(sectionManifest: ManifestSection) {
		this.#manifest = new BehaviorSubject<ManifestSection>(sectionManifest);
		this.manifest = this.#manifest.asObservable();
	}

	public setManifest(data: ManifestSection) {
		this.#manifest.next({ ...data });
	}

	public getData() {
		return this.#manifest.getValue();
	}

	/*
	This was not used anywhere
	public setActiveTree(tree: ManifestTree) {
		this._activeTree.next(tree);
	}
	*/

	public setActiveTreeItem(item: ActiveTreeItemType) {
		this.#activeTreeItem.next(item);
	}

	public setActiveView(view: ManifestSectionView) {
		this.#activeViewPathname.next(view.meta.pathname);
	}
}

export const UMB_SECTION_CONTEXT_TOKEN = new UmbContextToken<UmbSectionContext>(UmbSectionContext.name);
