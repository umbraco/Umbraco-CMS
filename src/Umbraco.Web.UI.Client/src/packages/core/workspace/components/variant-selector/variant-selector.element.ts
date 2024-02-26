import { UmbVariantId } from '../../../variant/variant-id.class.js';
import { UMB_PROPERTY_DATASET_CONTEXT, isNameablePropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import {
	type UUIInputElement,
	UUIInputEvent,
	type UUIPopoverContainerElement,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, type ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDocumentWorkspaceContext } from '@umbraco-cms/backoffice/document';

type UmbDocumentVariantOption = {
	culture: string | null;
	segment: string | null;
	title: string;
	displayName: string;
	state: DocumentVariantStateModel;
};

type UmbDocumentVariantOptions = Array<UmbDocumentVariantOption>;

@customElement('umb-variant-selector')
export class UmbVariantSelectorElement extends UmbLitElement {
	@query('#variant-selector-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _variants: UmbDocumentVariantOptions = [];

	// TODO: Stop using document context specific ActiveVariant type.
	@state()
	_activeVariants: Array<ActiveVariant> = [];

	@state()
	_activeVariantsCultures: string[] = [];

	#splitViewContext?: typeof UMB_WORKSPACE_SPLIT_VIEW_CONTEXT.TYPE;
	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	@state()
	private _name?: string;

	@state()
	private _variantDisplayName = '';

	@state()
	private _variantTitleName = '';

	@state()
	private _variantSelectorOpen = false;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, (instance) => {
			this.#splitViewContext = instance;
			this.#observeVariants();
			this.#observeActiveVariants();
			this.#observeCurrentVariant();
		});
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
			this.#datasetContext = instance;
			this.#observeDatasetContext();
			this.#observeCurrentVariant();
		});
	}

	async #observeVariants() {
		if (!this.#splitViewContext) return;

		// NOTICE: This is dirty (the TypeScript casting), we can only accept doing this so far because we currently only use the Variant Selector on Document Workspace. [NL]
		// This would need a refactor to enable the code below to work with different ContentTypes. Main problem here is the state, which is not generic for them all. [NL]
		const workspaceContext = this.#splitViewContext.getWorkspaceContext() as UmbDocumentWorkspaceContext;
		if (!workspaceContext) throw new Error('Split View Workspace context not found');

		this.observe(
			workspaceContext.variantOptions,
			(options) => {
				this._variants = options.map<UmbDocumentVariantOption>((option) => {
					const name = option.variant?.name ?? option.language.name;
					const segment = option.variant?.segment ?? null;
					return {
						// Notice the option object has a unique property, but it's not used here. (Its equivalent to a UmbVariantId string) [NL]
						culture: option.language.unique,
						segment: segment,
						title: name + (segment ? ` — ${segment}` : ''),
						displayName: name + (segment ? ` — ${segment}` : ''),
						state: option.variant?.state ?? DocumentVariantStateModel.NOT_CREATED,
					};
				});
			},
			'_observeVariants',
		);
	}

	async #observeActiveVariants() {
		if (!this.#splitViewContext) return;

		const workspaceContext = this.#splitViewContext.getWorkspaceContext();
		if (workspaceContext) {
			this.observe(
				workspaceContext.splitView.activeVariantsInfo,
				(activeVariants) => {
					if (activeVariants) {
						this._activeVariants = activeVariants;
						this._activeVariantsCultures = this._activeVariants.map((el) => el.culture ?? '') ?? [];
					}
				},
				'_observeActiveVariants',
			);
		}
	}

	async #observeDatasetContext() {
		if (!this.#datasetContext) return;
		this.observe(
			this.#datasetContext.name,
			(name) => {
				this._name = name;
			},
			'_name',
		);
	}

	async #observeCurrentVariant() {
		if (!this.#datasetContext || !this.#splitViewContext) return;
		const workspaceContext = this.#splitViewContext.getWorkspaceContext();
		if (!workspaceContext) return;

		const variantId = this.#datasetContext.getVariantId();
		// Find the variant option matching this, to get the language name...

		const culture = variantId.culture;
		const segment = variantId.segment;

		this.observe(
			workspaceContext.variantOptions,
			(options) => {
				const option = options.find((option) => option.language.unique === culture);
				const languageName = option?.language.name;
				this._variantDisplayName = (languageName ? languageName : '') + (segment ? ` — ${segment}` : '');
				this._variantTitleName =
					(languageName ? `${languageName} (${culture})` : '') + (segment ? ` — ${segment}` : '');
			},
			'_currentLanguage',
		);
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (
				typeof target?.value === 'string' &&
				this.#datasetContext &&
				isNameablePropertyDatasetContext(this.#datasetContext)
			) {
				this.#datasetContext.setName(target.value);
			}
		}
	}

	#switchVariant(variant: UmbDocumentVariantOption) {
		this.#splitViewContext?.switchVariant(UmbVariantId.Create(variant));
	}

	#openSplitView(variant: UmbDocumentVariantOption) {
		this.#splitViewContext?.openSplitView(UmbVariantId.Create(variant));
	}

	#closeSplitView() {
		this.#splitViewContext?.closeSplitView();
	}

	#isVariantActive(culture: string | null) {
		return culture !== null ? this._activeVariantsCultures.includes(culture) : true;
	}

	#isNotPublishedMode(culture: string | null, state: DocumentVariantStateModel) {
		return state !== DocumentVariantStateModel.PUBLISHED && !this.#isVariantActive(culture);
	}

	// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._variantSelectorOpen = event.newState === 'open';

		if (!this._popoverElement) return;

		const isOpen = event.newState === 'open';
		if (!isOpen) return;

		const host = this.getBoundingClientRect();
		// TODO: Ideally this is kept updated while open, but for now we just set it once:
		this._popoverElement.style.width = `${host.width}px`;
	}

	render() {
		return html`
			<uui-input id="name-input" .value=${this._name} @input="${this.#handleInput}">
				${
					this._variants?.length
						? html`
								<uui-button
									id="variant-selector-toggle"
									slot="append"
									popovertarget="variant-selector-popover"
									title=${this._variantTitleName}>
									${this._variantDisplayName}
									<uui-symbol-expand .open=${this._variantSelectorOpen}></uui-symbol-expand>
								</uui-button>
								${this._activeVariants.length > 1
									? html`
											<uui-button slot="append" compact id="variant-close" @click=${this.#closeSplitView}>
												<uui-icon name="remove"></uui-icon>
											</uui-button>
									  `
									: ''}
						  `
						: nothing
				}
			</uui-input>

			${
				this._variants?.length
					? html`
							<uui-popover-container
								id="variant-selector-popover"
								@beforetoggle=${this.#onPopoverToggle}
								placement="bottom-end">
								<div id="variant-selector-dropdown">
									<uui-scroll-container>
										<ul>
											${this._variants.map(
												(variant) => html`
													<li class="${this.#isVariantActive(variant.culture) ? 'selected' : ''}">
														<button
															class="variant-selector-switch-button
																	${this.#isNotPublishedMode(variant.culture, variant.state) ? 'add-mode' : ''}"
															@click=${() => this.#switchVariant(variant)}>
															${this.#isNotPublishedMode(variant.culture, variant.state)
																? html`<uui-icon class="add-icon" name="icon-add"></uui-icon>`
																: nothing}
															<div>
																${variant.title}
																<i>(${variant.culture})</i> ${variant.segment}
																<div class="variant-selector-state">${variant.state}</div>
															</div>
														</button>
														${this.#isVariantActive(variant.culture)
															? nothing
															: html`
																	<uui-button
																		class="variant-selector-split-view"
																		@click=${() => this.#openSplitView(variant)}>
																		Split view
																	</uui-button>
															  `}
													</li>
												`,
											)}
										</ul>
									</uui-scroll-container>
								</div>
							</uui-popover-container>
					  `
					: nothing
			}
		</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#name-input {
				width: 100%;
				height: 100%; /** I really don't know why this fixes the border colliding with variant-selector-toggle, but lets this solution for now */
			}

			#variant-selector-toggle {
				white-space: nowrap;
			}

			#variant-selector-popover {
				translate: 1px; /* Fixes tiny alignment issue caused by border */
			}

			#variant-selector-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}

			#variant-close {
				white-space: nowrap;
			}

			ul {
				list-style-type: none;
				padding: 0;
				margin: 0;
			}

			li {
				position: relative;
				margin-bottom: 1px;
			}

			li:nth-last-of-type(1) {
				margin-bottom: 0;
			}

			li.selected:before {
				background-color: var(--uui-color-current);
				border-radius: 0 4px 4px 0;
				bottom: 8px;
				content: '';
				left: 0;
				pointer-events: none;
				position: absolute;
				top: 8px;
				width: 4px;
				z-index: 1;
			}

			.variant-selector-switch-button {
				display: flex;
				align-items: center;
				border: none;
				background: transparent;
				color: var(--uui-color-current-contrast);
				padding: 6px 20px;
				font-weight: bold;
				width: 100%;
				text-align: left;
				font-size: 14px;
				cursor: pointer;
				border-bottom: 1px solid var(--uui-color-divider-standalone);
			}

			.variant-selector-switch-button:hover {
				background: var(--uui-palette-sand);
				color: var(--uui-palette-space-cadet-light);
			}

			.variant-selector-switch-button i {
				font-weight: normal;
			}

			.variant-selector-switch-button.add-mode {
				position: relative;
				color: var(--uui-palette-dusty-grey-dark);
			}

			.variant-selector-switch-button.add-mode:after {
				border: 2px dashed var(--uui-color-divider-standalone);
				bottom: 0;
				content: '';
				left: 0;
				margin: 2px;
				pointer-events: none;
				position: absolute;
				right: 0;
				top: 0;
				z-index: 1;
			}

			.add-icon {
				font-size: 12px;
				margin-right: 12px;
			}

			.variant-selector-split-view {
				position: absolute;
				top: 0;
				right: 0;
				bottom: 1px;
			}

			.variant-selector-state {
				color: var(--uui-palette-malibu-dimmed);
				font-size: 12px;
				font-weight: normal;
			}
		`,
	];
}

export default UmbVariantSelectorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-variant-selector': UmbVariantSelectorElement;
	}
}
