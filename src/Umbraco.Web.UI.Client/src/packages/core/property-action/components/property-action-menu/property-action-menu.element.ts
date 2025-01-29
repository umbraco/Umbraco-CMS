import type { UmbPropertyActionArgs } from '../../types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyAction, MetaPropertyAction } from '@umbraco-cms/backoffice/property-action';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

/**
 *
 * @param manifest
 * @returns
 */
function ExtensionApiArgsMethod(manifest: ManifestPropertyAction): [UmbPropertyActionArgs<MetaPropertyAction>] {
	return [{ meta: manifest.meta }];
}

@customElement('umb-property-action-menu')
export class UmbPropertyActionMenuElement extends UmbLitElement {
	@state()
	private _actions: Array<UmbExtensionElementAndApiInitializer<ManifestPropertyAction, never>> = [];

	@property()
	set propertyEditorUiAlias(alias: string) {
		this.#propertyEditorUiAlias = alias;

		// TODO: Stop using string for 'propertyAction', we need to start using Const. [NL]
		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'propertyAction',
			ExtensionApiArgsMethod,
			(propertyAction) => propertyAction.forPropertyEditorUis.includes(alias),
			(actions) => (this._actions = actions),
			'extensionsInitializer',
		);
	}
	get propertyEditorUiAlias() {
		return this.#propertyEditorUiAlias;
	}
	#propertyEditorUiAlias = '';

	override render() {
		if (!this._actions?.length) return nothing;
		return html`
			<uui-button id="popover-trigger" popovertarget="property-action-popover" label="Open actions menu" compact>
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
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			#more-symbol {
				font-size: 1rem;
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
