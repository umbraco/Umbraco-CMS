import { UMB_VARIANT_WORKSPACE_CONTEXT } from '../../../contexts/index.js';
import type { UmbVariantDatasetWorkspaceContext } from '../../../contexts/index.js';
import { css, customElement, html, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/menu';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import type { UmbVariantStructureItemModel } from '@umbraco-cms/backoffice/menu';

const observeDefaultLanguageSymbol = Symbol();
const observeCurrentLanguageSymbol = Symbol();
const observeWorkspaceActiveVariantSymbol = Symbol();
const observeWorkspaceNameSymbol = Symbol();

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

	@state()
	private _appCurrentCulture?: string;

	#workspaceContext?: UmbVariantDatasetWorkspaceContext;
	#appLanguageContext?: UmbAppLanguageContext;
	#menuStructureContext?: typeof UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.observe(
				this.#appLanguageContext?.appDefaultLanguage,
				(value) => {
					this._appDefaultCulture = value?.unique;
				},
				observeDefaultLanguageSymbol,
			);
			this.observe(
				this.#appLanguageContext?.appLanguageCulture,
				(value) => {
					this._appCurrentCulture = value;
				},
				observeCurrentLanguageSymbol,
			);
		});

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;
			this.#workspaceContext = instance;
			this.#observeWorkspaceActiveVariant();
			this.#observeStructure();
		});

		this.consumeContext(UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;
			this.#menuStructureContext = instance;
			this.#observeStructure();
		});
	}

	#observeStructure() {
		if (!this.#menuStructureContext || !this.#workspaceContext) return;

		this.observe(this.#menuStructureContext.structure, (value) => {
			if (!this.#workspaceContext) return;
			const unique = this.#workspaceContext.getUnique();
			// exclude the current unique from the structure. We append this with an observer of the name
			this._structure = value.filter((structureItem) => structureItem.unique !== unique);
		});
	}

	#observeWorkspaceActiveVariant() {
		this.observe(
			this.#workspaceContext?.splitView.firstActiveVariantInfo,
			(variantInfo) => {
				if (!variantInfo) return;
				this._workspaceActiveVariantId = UmbVariantId.Create(variantInfo);
				this.#observeActiveVariantName();
			},
			observeWorkspaceActiveVariantSymbol,
		);
	}

	#observeActiveVariantName() {
		this.observe(
			this.#workspaceContext?.name(this._workspaceActiveVariantId),
			(value) => (this._name = value || ''),
			observeWorkspaceNameSymbol,
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

		// Next try to find the variant that matches the current app culture.
		const variant = structureItem.variants.find(
			(variant) => variant.culture === this._appCurrentCulture && variant.segment === null,
		);
		if (variant) {
			if (this._workspaceActiveVariantId?.isInvariant()) {
				// If the active variant is invariant, we return the default name as the name without parentheses.
				// Cause it is the name, not an inherited/borrowed name. [NL]
				return variant.name;
			}
			return `(${variant.name})`;
		}

		// Next try to find the variant that matches the app default culture.
		const defaultVariant = structureItem.variants.find(
			(variant) => variant.culture === this._appDefaultCulture && variant.segment === null,
		);
		if (defaultVariant) {
			return `(${defaultVariant.name})`;
		}

		// Next try to find the invariant variant name.
		const invariantVariant = structureItem.variants.find(
			(variant) => variant.culture === null && variant.segment === null,
		);
		if (invariantVariant) {
			return invariantVariant.name;
		}

		// Last return the name of the first variant in the list.
		const lastResort = structureItem.variants?.[0]?.name;
		return lastResort ? `(${lastResort})` : '(#general_unknown)';
	}

	#getHref(structureItem: UmbVariantStructureItemModel) {
		return this.#menuStructureContext?.getItemHref(structureItem);
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
