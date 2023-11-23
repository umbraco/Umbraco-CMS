import { UmbPropertyActionMenuContext } from './property-action-menu.context.js';
import {
	css,
	CSSResultGroup,
	html,
	customElement,
	property,
	state,
	repeat,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	ManifestPropertyAction,
	ManifestTypes,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionElementInitializer, UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-property-action-menu')
export class UmbPropertyActionMenuElement extends UmbLitElement {
	#actionsInitializer?: UmbExtensionsElementInitializer<ManifestTypes, 'propertyAction'>;

	@property({ attribute: false })
	public get value(): unknown {
		return this._value;
	}
	public set value(value: unknown) {
		this._value = value;
		if (this.#actionsInitializer) {
			this.#actionsInitializer.properties = { value };
		}
	}
	private _value?: unknown;

	@property()
	set propertyEditorUiAlias(alias: string) {
		// TODO: Align property actions with entity actions.
		this.#actionsInitializer = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'propertyAction',
			(propertyAction) => propertyAction.meta.propertyEditors.includes(alias),
			(ctrls) => {
				this._actions = ctrls;
			},
		);
	}
	@state()
	private _actions: Array<UmbExtensionElementInitializer<ManifestPropertyAction, any>> = [];

	@state()
	private _open = false; //TODO: Is this still needed? now that we don't use the old popover anymore?

	//TODO: What is this context used for?
	//TODO: Is this still needed? now that we don't use the old popover anymore?
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
		//TODO: Is this still needed? now that we don't use the old popover anymore?
		this._propertyActionMenuContext.toggle();
	}

	private _handleClose(event: CustomEvent) {
		//TODO: Is this still needed? now that we don't use the old popover anymore?
		this._propertyActionMenuContext.close();
		event.stopPropagation();
	}

	render() {
		return this._actions.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="property-action-popover"
						look="secondary"
						label="More"
						@click="${this._toggleMenu}"
						compact>
						<uui-symbol-more id="more-symbol"></uui-symbol-more>
					</uui-button>
					<uui-popover-container id="property-action-popover">
						<umb-popover-layout>
							<div id="dropdown">${repeat(this._actions, (action) => action.component)}</div>
						</umb-popover-layout>
					</uui-popover-container>
			  `
			: nothing;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
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
		`,
	];
}
