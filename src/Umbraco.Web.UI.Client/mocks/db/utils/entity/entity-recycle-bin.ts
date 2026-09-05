import { UmbMockEntityRecycleBinTreeManager } from './entity-recycle-bin-tree.manager.js';

// Anchored on `name` (present on every variant type) so TS doesn't reject media's variants as a "weak type" match.
type UmbMockVariantWithState = { name: string; state?: string };

type UmbMockRecycleBinnableModel = {
	id: string;
	parent?: { id: string } | null;
	originalParent?: { id: string } | null;
	isTrashed: boolean;
	hasChildren: boolean;
	variants?: Array<UmbMockVariantWithState>;
};

/**
 * Always the parent DB itself, never a private copy — otherwise it drifts out of sync when the active mock set
 * changes.
 */
interface UmbMockRecycleBinSource<T> {
	getAll(): Array<T>;
	read(id: string): T | undefined;
	update(id: string, item: T): void;
}

/**
 * Recycle bin helper class for document/media DBs. Not a standalone DB — delegates to its parent DB for all
 * reads/writes. Mirrors the real server's trash/restore behaviour, including keeping each variant's `state` in
 * sync with `isTrashed`.
 */
export class UmbEntityRecycleBin<MockType extends UmbMockRecycleBinnableModel> {
	#db: UmbMockRecycleBinSource<MockType>;
	tree: UmbMockEntityRecycleBinTreeManager<MockType>;

	constructor(db: UmbMockRecycleBinSource<MockType>, treeItemMapper: (model: MockType) => any) {
		this.#db = db;
		this.tree = new UmbMockEntityRecycleBinTreeManager<MockType>(db, treeItemMapper);
	}

	read(id: string) {
		return this.#db.read(id);
	}

	trash(ids: string[]) {
		const models = ids.map((id) => this.read(id)).filter((model) => !!model) as Array<MockType>;

		models.forEach((model) => {
			if (model.isTrashed) return;
			model.originalParent = model.parent ?? null;
			model.parent = null;
			model.isTrashed = true;
			this.#setVariantsState(model, 'Trashed');
			this.#db.update(model.id, model);
		});

		models.forEach((model) => this.#trashDescendantsOf(model.id));
	}

	#trashDescendantsOf(parentId: string) {
		const children = this.#db.getAll().filter((item) => item.parent?.id === parentId);
		children.forEach((child) => {
			// No `originalParent` for descendants — restoring one "in place" is meaningless while its ancestor is
			// still trashed.
			child.isTrashed = true;
			this.#setVariantsState(child, 'Trashed');
			this.#db.update(child.id, child);
			this.#trashDescendantsOf(child.id);
		});
	}

	/** `destination`, when passed (including `null` for root), overrides the item's own `originalParent` fallback. */
	restore(ids: string[], destination?: { id: string } | null) {
		const models = ids.map((id) => this.read(id)).filter((model) => !!model) as Array<MockType>;

		models.forEach((model) => {
			if (!model.isTrashed) return;
			model.parent = destination !== undefined ? destination : (model.originalParent ?? null);
			model.originalParent = null;
			model.isTrashed = false;
			this.#setVariantsState(model, 'Draft');
			this.#db.update(model.id, model);
		});

		models.forEach((model) => this.#restoreDescendantsOf(model.id));
	}

	#restoreDescendantsOf(parentId: string) {
		const children = this.#db.getAll().filter((item) => item.parent?.id === parentId && item.isTrashed);
		children.forEach((child) => {
			child.isTrashed = false;
			this.#setVariantsState(child, 'Draft');
			this.#db.update(child.id, child);
			this.#restoreDescendantsOf(child.id);
		});
	}

	/** No-op for media — its variants never have a `state` field. */
	#setVariantsState(model: MockType, state: string) {
		model.variants?.forEach((variant) => {
			if ('state' in variant) variant.state = state;
		});
	}

	/** `undefined` for a descendant that was trashed only because its ancestor was — matches the real server. */
	getOriginalParent(id: string): { id: string } | null | undefined {
		const model = this.read(id);
		if (!model?.isTrashed) return undefined;
		// `null` (root) and `undefined` (never trashed itself) are different answers — don't collapse them.
		return model.originalParent;
	}
}
