import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { UmbBlockActionArgs } from './types.js';
import type { ManifestBlockAction, MetaBlockAction } from './block-action.extension.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-block-action-list')
export class UmbBlockActionListElement extends UmbLitElement {
	@property({ type: String, attribute: 'block-editor' })
	public blockEditor?: string;

	@state()
	private _showActions?: boolean;

	@state()
	private _actions: Array<UmbExtensionElementAndApiInitializer<ManifestBlockAction>> = [];

	#unique?: string;
	#contentTypeAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				observeMultiple([context.unique, context.contentElementTypeAlias, context.actionsVisibility]),
				([unique, contentTypeAlias, showActions]) => {
					this._showActions = showActions;
					this.#unique = unique ?? undefined;
					this.#contentTypeAlias = contentTypeAlias ?? undefined;
					this.#initExtensions();
				},
			);
		});
	}

	#extensionsInitialized = false;

	#initExtensions() {
		if (this.#extensionsInitialized || !this.#unique) return;
		this.#extensionsInitialized = true;

		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'blockAction',
			(manifest: ManifestBlockAction<MetaBlockAction>) =>
				[{ unique: this.#unique!, meta: manifest.meta }] as [UmbBlockActionArgs<MetaBlockAction>],
			(manifest: ManifestBlockAction<MetaBlockAction>) => this.#filterExtension(manifest),
			(actions) => (this._actions = actions),
			'blockActionsInitializer',
		);
	}

	#filterExtension(manifest: ManifestBlockAction): boolean {
		if (
			manifest.forContentTypeAlias &&
			!stringOrStringArrayContains(manifest.forContentTypeAlias, this.#contentTypeAlias ?? '')
		) {
			return false;
		}
		if (manifest.forBlockEditor && !stringOrStringArrayContains(manifest.forBlockEditor, this.blockEditor ?? '')) {
			return false;
		}
		return true;
	}

	override render() {
		if (!this._showActions) return nothing;

		return html`
			<uui-action-bar>
				${repeat(
					this._actions,
					(action) => action.alias,
					(action) => action.component,
				)}
			</uui-action-bar>
		`;
	}

	static override styles = [
		css`
			:host {
				position: absolute;
				top: var(--uui-size-2);
				right: var(--uui-size-2);
				z-index: 1;
			}

			uui-action-bar {
				opacity: var(--umb-block-entry-actions-opacity, 0);
				transition: opacity 120ms;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-action-list': UmbBlockActionListElement;
	}
}
