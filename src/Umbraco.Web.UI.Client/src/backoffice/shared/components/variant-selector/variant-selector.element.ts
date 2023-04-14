import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { ifDefined } from 'lit/directives/if-defined.js';
import {
	UmbWorkspaceVariantContext,
	UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN,
} from '../workspace/workspace-variant/workspace-variant.context';
import { ActiveVariant } from '../workspace/workspace-context/workspace-split-view-manager.class';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentVariantResponseModel, ContentStateModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-variant-selector')
export class UmbVariantSelectorElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#name-input {
				width: 100%;
				height: 100%; /** I really don't know why this fixes the border colliding with variant-selector-toggle, but lets this solution for now */
			}

			#variant-selector-toggle {
				white-space: nowrap;
			}

			#variant-selector-popover {
				display: block;
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
				font-family: Lato, Helvetica Neue, Helvetica, Arial, sans-serif;
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

	// TODO: not jet used:
	@property()
	alias!: string;

	@state()
	_variants: Array<DocumentVariantResponseModel> = [];

	// TODO: Stop using document context specific ActiveVariant type.
	@state()
	_activeVariants: Array<ActiveVariant> = [];

	@property()
	public get _activeVariantsCultures(): string[] {
		return this._activeVariants.map((el) => el.culture ?? '') ?? [];
	}

	private _variantContext?: UmbWorkspaceVariantContext;

	@state()
	private _name?: string;

	private _culture?: string | null;
	private _segment?: string | null;

	@state()
	private _variantDisplayName?: string;

	@state()
	private _variantTitleName?: string;

	// TODO: make adapt to backoffice locale.
	private _cultureNames = new Intl.DisplayNames('en', { type: 'language' });

	constructor() {
		super();

		this.consumeContext<UmbWorkspaceVariantContext>(UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN, (instance) => {
			this._variantContext = instance;
			this._observeVariants();
			this._observeActiveVariants();
			this._observeVariantContext();
		});
	}

	private async _observeVariants() {
		if (!this._variantContext) return;

		const workspaceContext = this._variantContext.getWorkspaceContext();
		if (workspaceContext) {
			this.observe(
				workspaceContext.variants,
				(variants) => {
					if (variants) {
						this._variants = variants;
					}
				},
				'_observeVariants'
			);
		}
	}

	private async _observeActiveVariants() {
		if (!this._variantContext) return;

		const workspaceContext = this._variantContext.getWorkspaceContext();
		if (workspaceContext) {
			this.observe(
				workspaceContext.splitView.activeVariantsInfo,
				(activeVariants) => {
					if (activeVariants) {
						this._activeVariants = activeVariants;
					}
				},
				'_observeActiveVariants'
			);
		}
	}

	private async _observeVariantContext() {
		if (!this._variantContext) return;

		this.observe(
			this._variantContext.name,
			(name) => {
				this._name = name;
			},
			'_name'
		);
		this.observe(
			this._variantContext.culture,
			(culture) => {
				this._culture = culture;
				this.updateVariantDisplayName();
			},
			'_culture'
		);
		this.observe(
			this._variantContext.segment,
			(segment) => {
				this._segment = segment;
				this.updateVariantDisplayName();
			},
			'_segment'
		);
	}

	private updateVariantDisplayName() {
		if (!this._culture && !this._segment) return;
		this._variantTitleName =
			(this._culture ? this._cultureNames.of(this._culture) + ` (${this._culture})` : '') +
			(this._segment ? ' — ' + this._segment : '');
		this._variantDisplayName =
			(this._culture ? this._cultureNames.of(this._culture) : '') + (this._segment ? ' — ' + this._segment : '');
	}

	// TODO: find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				// TODO: create a setName method on EntityWorkspace:
				this._variantContext?.setName(target.value);
			}
		}
	}

	private _toggleVariantSelector() {
		this._variantSelectorIsOpen = !this._variantSelectorIsOpen;
	}

	@state()
	private _variantSelectorIsOpen = false;

	private _close() {
		this._variantSelectorIsOpen = false;
	}

	private _switchVariant(variant: DocumentVariantResponseModel) {
		this._variantContext?.switchVariant(variant);
		this._close();
	}

	private _openSplitView(variant: DocumentVariantResponseModel) {
		this._variantContext?.openSplitView(variant);
		this._close();
	}

	private _closeSplitView() {
		this._variantContext?.closeSplitView();
	}

	private _isVariantActive(culture: string) {
		return this._activeVariantsCultures.includes(culture);
	}

	private _isNotPublishedMode(culture: string, state: ContentStateModel) {
		return state !== ContentStateModel.PUBLISHED && !this._isVariantActive(culture!);
	}

	render() {
		return html`
			<uui-input id="name-input" .value=${this._name} @input="${this._handleInput}">
				${
					this._variants && this._variants.length > 0
						? html`
								<uui-button
									slot="append"
									id="variant-selector-toggle"
									@click=${this._toggleVariantSelector}
									title=${ifDefined(this._variantTitleName)}>
									${this._variantDisplayName}
									<uui-symbol-expand></uui-symbol-expand>
								</uui-button>
								${this._activeVariants.length > 1
									? html`
											<uui-button slot="append" compact id="variant-close" @click=${this._closeSplitView}>
												<uui-icon name="remove"></uui-icon>
											</uui-button>
									  `
									: ''}
						  `
						: nothing
				}
			</uui-input>

			${
				this._variants && this._variants.length > 0
					? html`
							<uui-popover id="variant-selector-popover" .open=${this._variantSelectorIsOpen} @close=${this._close}>
								<div id="variant-selector-dropdown" slot="popover">
									<uui-scroll-container>
										<ul>
											${this._variants.map(
												(variant) =>
													html`
														<li class="${this._isVariantActive(variant.culture!) ? 'selected' : ''}">
															<button
																class="variant-selector-switch-button
																${this._isNotPublishedMode(variant.culture!, variant.state!) ? 'add-mode' : ''}"
																@click=${() => this._switchVariant(variant)}>
																${this._isNotPublishedMode(variant.culture!, variant.state!)
																	? html`<uui-icon class="add-icon" name="umb:add"></uui-icon>`
																	: nothing}
																<div>
																	${variant.name} <i>(${variant.culture})</i> ${variant.segment}
																	<div class="variant-selector-state">${variant.state}</div>
																</div>
															</button>

															${this._isVariantActive(variant.culture!)
																? nothing
																: html`
																		<uui-button
																			class="variant-selector-split-view"
																			@click=${() => this._openSplitView(variant)}>
																			Split view
																		</uui-button>
																  `}
														</li>
													`
											)}
										</ul>
									</uui-scroll-container>
								</div>
							</uui-popover>
					  `
					: nothing
			}
		</div>
		`;
	}
}

export default UmbVariantSelectorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-variant-selector': UmbVariantSelectorElement;
	}
}
