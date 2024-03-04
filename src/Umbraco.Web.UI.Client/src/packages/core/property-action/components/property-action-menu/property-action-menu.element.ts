import type { UmbPropertyActionArgs } from '../property-action/types.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	ManifestPropertyAction,
	ManifestTypes,
	MetaPropertyAction,
} from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

function ExtensionApiArgsMethod(manifest: ManifestPropertyAction): [UmbPropertyActionArgs<MetaPropertyAction>] {
	return [{ meta: manifest.meta }];
}
@customElement('umb-property-action-menu')
export class UmbPropertyActionMenuElement extends UmbLitElement {
	#actionsInitializer?: UmbExtensionsElementAndApiInitializer<ManifestTypes, 'propertyAction'>;
	#propertyEditorUiAlias = '';

	@property()
	set propertyEditorUiAlias(alias: string) {
		this.#propertyEditorUiAlias = alias;
		// TODO: Stop using string for 'propertyAction', we need to start using Const. [NL]
		this.#actionsInitializer = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'propertyAction',
			ExtensionApiArgsMethod,
			(propertyAction) => propertyAction.forPropertyEditorUis.includes(alias),
			(ctrls) => {
				this._actions = ctrls;
			},
			'extensionsInitializer',
		);
	}
	get propertyEditorUiAlias() {
		return this.#propertyEditorUiAlias;
	}

	@state()
	private _actions: Array<UmbExtensionElementAndApiInitializer<ManifestPropertyAction, never>> = [];

	render() {
		return this._actions.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="property-action-popover"
						look="secondary"
						label="Open actions menu"
						compact>
						<uui-symbol-more id="more-symbol"></uui-symbol-more>
					</uui-button>
					<uui-popover-container id="property-action-popover">
						<umb-popover-layout>
							${repeat(
								this._actions,
								(action) => action.alias,
								(action) => action.component,
							)}
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-action-menu': UmbPropertyActionMenuElement;
	}
}
