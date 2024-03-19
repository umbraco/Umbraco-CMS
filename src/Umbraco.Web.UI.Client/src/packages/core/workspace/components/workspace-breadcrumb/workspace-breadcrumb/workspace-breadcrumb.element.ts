import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbMenuStructureWorkspaceContext, UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';

@customElement('umb-workspace-breadcrumb')
export class UmbWorkspaceBreadcrumbElement extends UmbLitElement {
	// TODO: figure out the correct context type
	#workspaceContext?: any;

	@state()
	_isNew = false;

	@state()
	_name: string = '';

	@state()
	_structure: UmbStructureItemModel[] = [];

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as any;
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.observe(this.#workspaceContext.name, (value) => (this._name = value), 'breadcrumbWorkspaceNameObserver');
		});

		// TODO: set up context token
		this.consumeContext('UmbMenuStructureWorkspaceContext', (instance) => {
			// TODO: get the correct interface from the context token
			const context = instance as UmbMenuStructureWorkspaceContext;
			this.observe(context.structure, (value) => (this._structure = value), 'menuStructureObserver');
		});

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
		});
	}

	#getHref(structureItem: UmbStructureItemModel) {
		const workspaceBasePath = `section/${this.#sectionContext?.getPathname()}/workspace/${structureItem.entityType}/edit`;
		return structureItem.isFolder ? undefined : `${workspaceBasePath}/${structureItem.unique}`;
	}

	render() {
		return html`
			<uui-breadcrumbs>
				${this._structure?.map(
					(structureItem) =>
						html`<uui-breadcrumb-item href="${ifDefined(this.#getHref(structureItem))}"
							>${structureItem.name}</uui-breadcrumb-item
						>`,
				)}
			</uui-breadcrumbs>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbWorkspaceBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-breadcrumb': UmbWorkspaceBreadcrumbElement;
	}
}
