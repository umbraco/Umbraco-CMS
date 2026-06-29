import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { ManifestWorkspaceViewTreeKind } from './types.js';
import { html, nothing, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_INTERACTION_MEMORY_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbTreeElement } from '../tree.element.js';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-tree-workspace-view')
export class UmbTreeWorkspaceViewElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public manifest?: ManifestWorkspaceViewTreeKind;

	@state()
	private _parent?: UmbEntityModel;

	@state()
	private _interactionMemories?: Array<UmbInteractionMemoryModel>;

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			const entityType = context?.getEntityType();
			const unique = context?.getUnique();
			this._parent = entityType && unique !== undefined ? { entityType, unique } : undefined;
		});

		this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
			this.#interactionMemoryContext = context;
			this.#readInteractionMemory();
		});
	}

	protected override updated(changedProperties: PropertyValues) {
		super.updated(changedProperties);
		if (changedProperties.has('manifest')) {
			this.#readInteractionMemory();
		}
	}

	#readInteractionMemory() {
		const alias = this.manifest?.alias;
		if (!alias || !this.#interactionMemoryContext) return;
		const stored = this.#interactionMemoryContext.memory.getMemory(alias);
		this._interactionMemories = stored?.memories ?? [];
	}

	#onInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const alias = this.manifest?.alias;
		if (!alias || !this.#interactionMemoryContext) return;
		const tree = event.currentTarget as UmbTreeElement;
		const memories = tree.interactionMemories;
		if (memories.length > 0) {
			this.#interactionMemoryContext.memory.setMemory({ unique: alias, memories });
		} else {
			this.#interactionMemoryContext.memory.deleteMemory(alias);
		}
	}

	override render() {
		if (!this.manifest) return html` <div>Missing Workspace View Manifest</div>`;
		if (!this.manifest.meta.treeAlias)
			return html` <div>Missing Tree Alias as part of this Workspace View Manifest</div>`;
		if (this._parent === undefined) return nothing;
		return html`<umb-tree
			data-mark="tree:${this.manifest.meta.treeAlias}"
			alias=${this.manifest.meta.treeAlias}
			.props=${{
				hideToolbar: false,
				hideTreeActions: false,
				hideTreeRoot: true,
				startNode: this._parent,
				interactionMemories: this._interactionMemories,
				selectionConfiguration: {
					selectable: false,
				},
			}}
			@interaction-memories-change=${this.#onInteractionMemoriesChange}></umb-tree>`;
	}

	static override styles = css`
		:host {
			display: block;
			padding: var(--uui-size-layout-1);
		}
	`;
}

export { UmbTreeWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-workspace-view': UmbTreeWorkspaceViewElement;
	}
}
