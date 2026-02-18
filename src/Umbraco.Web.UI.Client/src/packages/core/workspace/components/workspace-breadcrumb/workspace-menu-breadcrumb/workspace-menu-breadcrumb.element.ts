import { UMB_WORKSPACE_CONTEXT } from '../../../workspace.context-token.js';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '../../../paths.js';
import { css, customElement, html, ifDefined, map, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/menu';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbMenuStructureWorkspaceContext, UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';

@customElement('umb-workspace-breadcrumb')
export class UmbWorkspaceBreadcrumbElement extends UmbLitElement {
	@state()
	private _name: string = '';

	@state()
	private _structure: UmbStructureItemModel[] = [];

	@state()
	private _isNew: boolean = false;

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
			this.#observeIsNew();
			this.#observeStructure();
			this.#observeName();
		});

		this.consumeContext<UmbMenuStructureWorkspaceContext>(UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT, (instance) => {
			this.#structureContext = instance;
			this.#observeStructure();
		});
	}

	#observeIsNew() {
		this.observe(
			this.#workspaceContext?.isNew,
			(value) => {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				this._isNew = value || false;
			},
			'breadcrumbWorkspaceIsNewObserver',
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

	#observeStructure() {
		if (!this.#structureContext || !this.#workspaceContext) return;

		this.observe(
			this.#structureContext.structure,
			(value) => {
				this._structure = value;
			},
			'menuStructureObserver',
		);
	}

	#getHref(structureItem: UmbStructureItemModel) {
		if (structureItem.isFolder || !structureItem.unique) return undefined;

		const sectionName = this.#sectionContext?.getPathname();
		if (!sectionName) return undefined;

		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName,
			entityType: structureItem.entityType,
			unique: structureItem.unique,
		});
	}

	override render() {
		const structure = this._isNew ? this._structure : this._structure.slice(0, -1);

		return html`
			<uui-breadcrumbs>
				${map(
					structure,
					(structureItem) =>
						html`<uui-breadcrumb-item href=${ifDefined(this.#getHref(structureItem))}
							>${this.localize.string(structureItem.name)}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item last-item>${this._name}</uui-breadcrumb-item>
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
