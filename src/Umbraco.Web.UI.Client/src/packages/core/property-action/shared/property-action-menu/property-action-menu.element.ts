import { UmbPropertyActionMenuContext } from './property-action-menu.context.js';
import { css, CSSResultGroup, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { ManifestPropertyAction, ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionElementInitializer, UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../property-action/property-action.element.js';

@customElement('umb-property-action-menu')
export class UmbPropertyActionMenuElement extends UmbLitElement {

	#actionsInitializer?: UmbExtensionsElementInitializer<ManifestTypes, 'propertyAction'>;


	@property({ attribute: false })
	public get value(): unknown {
		return this._value;
	}
	public set value(value: unknown) {
		this._value = value;
		if(this.#actionsInitializer) {
			this.#actionsInitializer.properties = { value };
		}
	}
	private _value?: unknown;

	@property()
	set propertyEditorUiAlias(alias: string) {

		// TODO: Align property actions with entity actions.
		this.#actionsInitializer = new UmbExtensionsElementInitializer(this, umbExtensionsRegistry, 'propertyAction', (propertyAction) => propertyAction.meta.propertyEditors.includes(alias), (ctrls) => {
			this._actions = ctrls;
		});
	}
	@state()
	private _actions: Array<UmbExtensionElementInitializer<ManifestPropertyAction, any>> = [];

	@state()
	private _open = false;

	private _propertyActionMenuContext = new UmbPropertyActionMenuContext(this);

	constructor() {
		super();

		this.observe(this._propertyActionMenuContext.isOpen, (isOpen) => {
			this._open = isOpen;
		});

		this.addEventListener('close', (e) => {
			this._propertyActionMenuContext.close();
			e.stopPropagation();
		});
	}

	private _toggleMenu() {
		this._propertyActionMenuContext.toggle();
	}

	private _handleClose(event: CustomEvent) {
		this._propertyActionMenuContext.close();
		event.stopPropagation();
	}

	render() {
		return this._actions.length > 0
			? html`
					<uui-popover id="popover" placement="bottom-start" .open=${this._open} @close="${this._handleClose}">
						<uui-button
							id="popover-trigger"
							slot="trigger"
							look="secondary"
							label="More"
							@click="${this._toggleMenu}"
							compact>
							<uui-symbol-more id="more-symbol"></uui-symbol-more>
						</uui-button>

						<div slot="popover" id="dropdown">
							${repeat(this._actions,
								(action) => action.component
							)}
						</div>
					</uui-popover>
			  `
			: '';
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			#popover {
				width: auto;
			}

			#more-symbol {
				font-size: 0.6em;
			}

			#popover-trigger {
				--uui-button-padding-top-factor: 0.5;
				--uui-button-padding-bottom-factor: 0.1;
				--uui-button-height: 18px;
				--uui-button-border-radius: 6px;
			}

			#dropdown {
				background-color: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
				min-width: 200px;
				color: var(--uui-color-text);
			}
		`,
	];
}
