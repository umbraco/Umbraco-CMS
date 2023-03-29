import { css, CSSResultGroup, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { UUITextStyles } from '@umbraco-ui/uui';
import { UmbPropertyActionMenuContext } from './property-action-menu.context';
import type { ManifestPropertyAction } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

import '../property-action/property-action.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-property-action-menu')
export class UmbPropertyActionMenuElement extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
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

	// TODO: we need to investigate context api vs values props and events
	@property()
	public value?: string;

	@property()
	set propertyEditorUiAlias(alias: string) {
		this._observeActions(alias);
	}

	private _actionsObserver?: UmbObserverController<ManifestPropertyAction[]>;

	@state()
	private _actions: Array<ManifestPropertyAction> = [];

	@state()
	private _open = false;

	private _propertyActionMenuContext = new UmbPropertyActionMenuContext(this);

	constructor() {
		super();

		this.observe(this._propertyActionMenuContext.isOpen, (value) => {
			this._open = value;
		});

		this.addEventListener('close', (e) => {
			this._propertyActionMenuContext.close();
			e.stopPropagation();
		});
	}

	private _observeActions(alias: string) {
		this._actionsObserver?.destroy();
		this._actionsObserver = this.observe(
			umbExtensionsRegistry.extensionsOfType('propertyAction').pipe(
				map((propertyActions) => {
					return propertyActions.filter((propertyAction) => propertyAction.conditions.propertyEditors.includes(alias));
				})
			),
			(manifests) => {
				this._actions = manifests;
			}
		);
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
							${this._actions.map(
								(action) => html`
									<umb-property-action .propertyAction=${action} .value="${this.value}"></umb-property-action>
								`
							)}
						</div>
					</uui-popover>
			  `
			: '';
	}
}
