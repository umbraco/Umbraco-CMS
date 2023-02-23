import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbWorkspaceVariantContext } from '../workspace/workspace-variant/workspace-variant.context';
import { UmbDocumentWorkspaceContext } from '../../../documents/documents/workspace/document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DocumentVariantModel } from '@umbraco-cms/backend-api';

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
		`,
	];

	// TODO: not jet used:
	@property()
	alias!: string;

	@state()
	_variants: Array<DocumentVariantModel> = [];

	private _workspaceContext?: UmbDocumentWorkspaceContext;
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

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (instance) => {
			this._workspaceContext = instance;
			this._observeVariants();
		});

		this.consumeContext<UmbWorkspaceVariantContext>('umbWorkspaceVariantContext', (instance) => {
			this._variantContext = instance;
			this._observeVariantContext();
		});
	}

	private async _observeVariants() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.variants, (variants) => {
			if (variants) {
				this._variants = variants;
			}
		});
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

	// TODO. find a way where we don't have to do this for all workspaces.
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

	private _switchVariant(variant: DocumentVariantModel) {
		if (variant.culture === undefined || variant.segment === undefined) return;
		this._variantContext?.changeVariant(variant.culture, variant.segment);
		this._close();
	}

	private _openSplitView(variant: DocumentVariantModel) {
		if (variant.culture === undefined || variant.segment === undefined) return;
		this._workspaceContext?.openSplitView(variant.culture, variant.segment);
		this._close();
	}

	render() {
		return html`
			<uui-input id="name-input" .value=${this._name} @input="${this._handleInput}">
				${
					this._variants && this._variants.length > 0
						? html`
								<div slot="append">
									<uui-button
										id="variant-selector-toggle"
										@click=${this._toggleVariantSelector}
										title=${ifDefined(this._variantTitleName)}>
										${this._variantDisplayName}
										<uui-caret></uui-caret>
									</uui-button>
								</div>
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
										${this._variants.map(
											(variant) =>
												html`<ul>
													<li>
														<uui-button @click=${() => this._switchVariant(variant)}>
															${variant.name} ${variant.culture} ${variant.segment}
														</uui-button>

														<uui-button @click=${() => this._openSplitView(variant)}> Split view </uui-button>
													</li>
												</ul>`
										)}
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
