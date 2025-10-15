import { UMB_VARIANT_WORKSPACE_CONTEXT } from '../../../contexts/index.js';
import type { UmbVariantDatasetWorkspaceContext } from '../../../contexts/index.js';
import { css, customElement, html, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/menu';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import type { UmbVariantStructureItemModel } from '@umbraco-cms/backoffice/menu';

@customElement('umb-workspace-variant-menu-breadcrumb')
export class UmbWorkspaceVariantMenuBreadcrumbElement extends UmbLitElement {
	@state()
	private _name: string = '';

	@state()
	private _structure: Array<UmbVariantStructureItemModel> = [];

	@state()
	private _workspaceActiveVariantId?: UmbVariantId;

	@state()
	private _appDefaultCulture?: string;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#workspaceContext?: UmbVariantDatasetWorkspaceContext;
	#appLanguageContext?: UmbAppLanguageContext;
	#structureContext?: typeof UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT.TYPE;

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

		this.consumeContext(UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;
			this.#structureContext = instance;
			this.#observeStructure();
		});
	}

	#observeStructure() {
		if (!this.#structureContext || !this.#workspaceContext) return;

		this.observe(this.#structureContext.structure, (value) => {
			if (!this.#workspaceContext) return;
			const unique = this.#workspaceContext.getUnique();
			// exclude the current unique from the structure. We append this with an observer of the name
			this._structure = value.filter((structureItem) => structureItem.unique !== unique);
		});
	}

	#observeDefaultCulture() {
		this.observe(this.#appLanguageContext?.appDefaultLanguage, (value) => {
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
		// If the active workspace is a variant, we will try to find the matching variant name.
		if (!this._workspaceActiveVariantId?.isInvariant()) {
			const variant = structureItem.variants.find((variantId) => this._workspaceActiveVariantId?.compare(variantId));
			if (variant) {
				return variant.name;
			}
		}

		// If the active workspace is invariant, we will try to find the variant that matches the app default culture.
		const variant = structureItem.variants.find((variant) => variant.culture === this._appDefaultCulture);
		if (variant) {
			return `(${variant.name})`;
		}

		// If an active variant can not be found, then we fallback to the first variant name or a generic "unknown" label.
		return structureItem.variants?.[0]?.name ?? '(#general_unknown)';
	}

	#getHref(structureItem: any) {
		if (structureItem.isFolder) return undefined;

		let href = `section/${this.#sectionContext?.getPathname()}`;
		if (structureItem.unique) {
			href += `/workspace/${structureItem.entityType}/edit/${structureItem.unique}/${this._workspaceActiveVariantId?.toCultureString()}`;
		}

		return href;
	}

	override render() {
		return html`
			<uui-breadcrumbs>
				${this._structure.map(
					(structureItem) =>
						html`<uui-breadcrumb-item href=${ifDefined(this.#getHref(structureItem))}
							>${this.localize.string(this.#getItemVariantName(structureItem))}</uui-breadcrumb-item
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
