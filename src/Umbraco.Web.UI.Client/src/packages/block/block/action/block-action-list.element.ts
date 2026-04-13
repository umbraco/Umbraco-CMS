import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { UmbBlockActionArgs } from './types.js';
import type { ManifestBlockAction, MetaBlockAction } from './block-action.extension.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbApiConstructorArgumentsMethodType } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-block-action-list')
export class UmbBlockActionListElement extends UmbLitElement {
	@property({ type: String, attribute: 'block-editor' })
	public blockEditor?: string;

	@state()
	private _showActions?: boolean;

	@state()
	private _filter?: (manifest: ManifestBlockAction<MetaBlockAction>) => boolean;

	@state()
	private _apiArgs?: UmbApiConstructorArgumentsMethodType<
		ManifestBlockAction<MetaBlockAction>,
		[UmbBlockActionArgs<MetaBlockAction>]
	>;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				observeMultiple([context.unique, context.contentElementTypeAlias, context.actionsVisibility]),
				([unique, contentTypeAlias, showActions]) => {
					this._showActions = showActions;

					if (!unique) {
						this._filter = undefined;
						this._apiArgs = undefined;
						return;
					}

					this._filter = (manifest: ManifestBlockAction<MetaBlockAction>) => {
						if (
							manifest.forContentTypeAlias &&
							!stringOrStringArrayContains(manifest.forContentTypeAlias, contentTypeAlias ?? '')
						) {
							return false;
						}
						if (
							manifest.forBlockEditor &&
							!stringOrStringArrayContains(manifest.forBlockEditor, this.blockEditor ?? '')
						) {
							return false;
						}
						return true;
					};

					this._apiArgs = (manifest: ManifestBlockAction<MetaBlockAction>) => {
						return [{ unique, meta: manifest.meta }];
					};
				},
			);
		});
	}

	override render() {
		if (!this._showActions) return nothing;
		if (!this._filter) return nothing;

		return html`
			<uui-action-bar>
				<slot></slot>
				<umb-extension-with-api-slot
					type="blockAction"
					.filter=${this._filter}
					.apiArgs=${this._apiArgs}
					.renderMethod=${(ext: { component: HTMLElement }) => ext.component}>
				</umb-extension-with-api-slot>
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
