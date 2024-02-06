import type {
	CSSResultGroup} from '@umbraco-cms/backoffice/external/lit';
import {
	css,
	html,
	customElement,
	property,
	state,
	repeat,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	ManifestPropertyAction,
	ManifestTypes} from '@umbraco-cms/backoffice/extension-registry';
import {
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementInitializer} from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
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
		// TODO: Stop using string for 'propertyAction', we need to start using Const.
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

	render() {
		return this._actions.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="property-action-popover"
						look="secondary"
						label="More"
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
