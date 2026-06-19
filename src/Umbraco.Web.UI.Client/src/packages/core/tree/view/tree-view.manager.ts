import type { ManifestTreeView } from './tree-view.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';

/**
 * Fallback used when no `treeView` manifests are registered.
 * Trees should register at least one treeView manifest using `kind: 'classic'`.
 * @deprecated Register a treeView manifest with `kind: 'classic'` on your tree instead.
 */
const CLASSIC_FALLBACK: ManifestTreeView = {
	type: 'treeView',
	kind: 'classic',
	alias: 'Umb.TreeView.Classic.Fallback',
	name: 'Classic Tree View (fallback)',
	element: () => import('./classic/classic-tree-view.element.js'),
	weight: 0,
	meta: {
		label: '#tree_classicViewLabel',
		icon: 'icon-blockquote',
	},
};

const MEMORY_UNIQUE = 'UmbTreeCurrentView';

export interface UmbTreeViewManagerArgs {
	interactionMemoryManager?: UmbInteractionMemoryManager;
}

export class UmbTreeViewManager extends UmbControllerBase {
	#views = new UmbArrayState<ManifestTreeView>([], (x) => x.alias);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestTreeView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	#treeAlias?: string;
	#extensionsInitializer?: UmbExtensionsManifestInitializer<any, any>;

	#interactionMemoryManager?: UmbInteractionMemoryManager;
	#muteMemoryObservation = false;

	constructor(host: UmbControllerHost, args?: UmbTreeViewManagerArgs) {
		super(host);
		this.#interactionMemoryManager = args?.interactionMemoryManager;

		if (this.#interactionMemoryManager) {
			this.#observeInteractionMemory();
		}
	}

	setTreeAlias(treeAlias: string) {
		if (this.#treeAlias === treeAlias) return;
		this.#treeAlias = treeAlias;
		this.#extensionsInitializer?.destroy();

		this.#extensionsInitializer = new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'treeView',
			(manifest: ManifestTreeView) => !manifest.forTrees?.length || manifest.forTrees.includes(treeAlias),
			(result) => {
				const views = result.map((v) => v.manifest);

				this.#views.setValue(views);

				if (!views.length) {
					// No treeView manifests registered for this tree — use the built-in classic fallback.
					new UmbDeprecation({
						removeInVersion: '20.0.0',
						deprecated: 'Implicit classic tree view fallback',
						solution:
							"Register a treeView manifest with `kind: 'classic'` on your tree. The automatic fallback will be removed in Umbraco 19.",
					}).warn();
					this.#currentView.setValue(CLASSIC_FALLBACK);
					return;
				}

				const storedAlias = this.#interactionMemoryManager?.getMemory(MEMORY_UNIQUE)?.value?.alias;
				const initialView = storedAlias ? (views.find((v) => v.alias === storedAlias) ?? views[0]) : views[0];

				this.#writeToMemory(initialView);
				this.#currentView.setValue(initialView);
			},
		);
	}

	setCurrentView(view: ManifestTreeView) {
		this.#writeToMemory(view);
		this.#currentView.setValue(view);
	}

	getCurrentView(): ManifestTreeView | undefined {
		return this.#currentView.getValue();
	}

	#writeToMemory(view: ManifestTreeView) {
		if (!this.#interactionMemoryManager) return;
		const memory: UmbInteractionMemoryModel = { unique: MEMORY_UNIQUE, value: { alias: view.alias } };
		this.#muteMemoryObservation = true;
		this.#interactionMemoryManager.setMemory(memory);
		this.#muteMemoryObservation = false;
	}

	#observeInteractionMemory() {
		this.observe(
			this.#interactionMemoryManager!.memory(MEMORY_UNIQUE),
			(memory) => {
				if (this.#muteMemoryObservation) return;
				if (!memory) return;
				const views = this.#views.getValue();
				if (!views.length) return; // extensions not loaded yet; initializer callback will handle it
				const match = views.find((v) => v.alias === memory.value?.alias);
				if (match) {
					this.#currentView.setValue(match);
				}
			},
			'umbTreeViewMemoryObserver',
		);
	}
}
