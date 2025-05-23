import { UMB_WORKSPACE_CONTEXT } from '../../../workspace.context-token.js';
import { css, customElement, html, ifDefined, map, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import {
	UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT,
	type UmbMenuStructureWorkspaceContext,
	type UmbStructureItemModel,
} from '@umbraco-cms/backoffice/menu';

@customElement('umb-workspace-breadcrumb')
export class UmbWorkspaceBreadcrumbElement extends UmbLitElement {
	@state()
	_name: string = '';

	@state()
	_structure: UmbStructureItemModel[] = [];

	// TODO: figure out the correct context type
	#workspaceContext?: any;
	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#structureContext?: typeof UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
		});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as any;
			this.#observeStructure();
			this.#observeName();
		});

		this.consumeContext<UmbMenuStructureWorkspaceContext>(UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT, (instance) => {
			this.#structureContext = instance;
			this.#observeStructure();
		});
	}

	#observeStructure() {
		if (!this.#structureContext || !this.#workspaceContext) return;
		const isNew = this.#workspaceContext.getIsNew();

		this.observe(
			this.#structureContext.structure,
			(value) => {
				this._structure = isNew ? value : value.slice(0, -1);
			},
			'menuStructureObserver',
		);
	}

	#observeName() {
		this.observe(
			this.#workspaceContext?.name,
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			(value) => (this._name = value || ''),
			'breadcrumbWorkspaceNameObserver',
		);
	}

	#getHref(structureItem: UmbStructureItemModel) {
		const workspaceBasePath = `section/${this.#sectionContext?.getPathname()}/workspace/${structureItem.entityType}/edit`;
		return structureItem.isFolder ? undefined : `${workspaceBasePath}/${structureItem.unique}`;
	}

	override render() {
		return html`
			<uui-breadcrumbs>
				${map(
					this._structure,
					(structureItem) =>
						html`<uui-breadcrumb-item href=${ifDefined(this.#getHref(structureItem))}
							>${this.localize.string(structureItem.name)}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item>${this._name}</uui-breadcrumb-item>
			</uui-breadcrumbs>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				margin-left: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbWorkspaceBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-breadcrumb': UmbWorkspaceBreadcrumbElement;
	}
}
