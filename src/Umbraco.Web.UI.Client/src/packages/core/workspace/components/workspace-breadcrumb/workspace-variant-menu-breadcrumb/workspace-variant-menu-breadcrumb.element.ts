import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbVariantableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbVariantStructureItemModel } from '@umbraco-cms/backoffice/menu';

@customElement('umb-workspace-variant-menu-breadcrumb')
export class UmbWorkspaceVariantMenuBreadcrumbElement extends UmbLitElement {
	@state()
	_name: string = '';

	@state()
	_structure: Array<UmbVariantStructureItemModel> = [];

	@state()
	_workspaceActiveVariantId?: UmbVariantId;

	@state()
	_appDefaultCulture?: string;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#workspaceContext?: UmbVariantableWorkspaceContextInterface<UmbVariantModel>;
	#appLanguageContext?: UmbAppLanguageContext;
	#structureContext?: any;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.#observeDefaultCulture();
		});

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
		});

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;
			this.#workspaceContext = instance;
			this.#observeWorkspaceActiveVariant();
			this.#observeStructure();
		});

		// TODO: set up context token
		this.consumeContext('UmbMenuStructureWorkspaceContext', (instance) => {
			if (!instance) return;
			this.#structureContext = instance;
			this.#observeStructure();
		});
	}

	#observeStructure() {
		if (!this.#structureContext || !this.#workspaceContext) return;
		const isNew = this.#workspaceContext.getIsNew();

		this.observe(this.#structureContext.structure, (value) => {
			// TODO: get the type from the context
			const structure = value as Array<UmbVariantStructureItemModel>;
			this._structure = isNew ? structure : structure.slice(0, -1);
		});
	}

	#observeDefaultCulture() {
		this.observe(this.#appLanguageContext!.appDefaultLanguage, (value) => {
			this._appDefaultCulture = value?.unique;
		});
	}

	#observeWorkspaceActiveVariant() {
		this.observe(
			this.#workspaceContext?.splitView.activeVariantsInfo,
			(value) => {
				if (!value) return;
				this._workspaceActiveVariantId = UmbVariantId.Create(value[0]);
				this.#observeActiveVariantName();
			},

			'breadcrumbWorkspaceActiveVariantObserver',
		);
	}

	#observeActiveVariantName() {
		this.observe(
			this.#workspaceContext?.name(this._workspaceActiveVariantId),
			(value) => (this._name = value || ''),
			'breadcrumbWorkspaceNameObserver',
		);
	}

	// TODO: we should move the fallback name logic to a helper class. It will be used in multiple places
	#getItemVariantName(structureItem: UmbVariantStructureItemModel) {
		const fallbackName =
			structureItem.variants.find((variant) => variant.culture === this._appDefaultCulture)?.name ??
			structureItem.variants[0].name ??
			'Unknown';
		const name = structureItem.variants.find((variant) => this._workspaceActiveVariantId?.compare(variant))?.name;
		return name ?? `(${fallbackName})`;
	}

	#getHref(structureItem: any) {
		const workspaceBasePath = `section/${this.#sectionContext?.getPathname()}/workspace/${structureItem.entityType}/edit`;
		return structureItem.isFolder
			? undefined
			: `${workspaceBasePath}/${structureItem.unique}/${this._workspaceActiveVariantId?.culture}`;
	}

	render() {
		return html`
			<uui-breadcrumbs>
				${this._structure.map(
					(structureItem) =>
						html`<uui-breadcrumb-item href="${ifDefined(this.#getHref(structureItem))}"
							>${this.#getItemVariantName(structureItem)}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item>${this._name}</uui-breadcrumb-item>
			</uui-breadcrumbs>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbWorkspaceVariantMenuBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-variant-menu-breadcrumb': UmbWorkspaceVariantMenuBreadcrumbElement;
	}
}
