import { UMB_VARIANT_WORKSPACE_CONTEXT, type UmbVariantDatasetWorkspaceContext } from '../../../contexts/index.js';
import { css, customElement, html, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
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
	#workspaceContext?: UmbVariantDatasetWorkspaceContext;
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

	override render() {
		return html`
			<uui-breadcrumbs>
				${this._structure.map(
					(structureItem) =>
						html`<uui-breadcrumb-item href="${ifDefined(this.#getHref(structureItem))}"
							>${this.localize.string(this.#getItemVariantName(structureItem))}</uui-breadcrumb-item
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
				/* TODO: This is a temp solution to handle an issue where long nested breadcrumbs would hide workspace actions */
				overflow: hidden;
				display: flex;
				flex-direction: row-reverse;
				margin-left: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbWorkspaceVariantMenuBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-variant-menu-breadcrumb': UmbWorkspaceVariantMenuBreadcrumbElement;
	}
}
