import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import type { ManifestWorkspaceAction, MetaWorkspaceAction } from '../../extensions/types.js';
import type { UmbWorkspaceActionArgs } from '../workspace-action/types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';

/**
 *
 * @param manifest
 * @returns
 */
function ExtensionApiArgsMethod(
	manifest: ManifestWorkspaceAction<MetaWorkspaceAction>,
): [UmbWorkspaceActionArgs<MetaWorkspaceAction>] {
	return [{ meta: manifest.meta }];
}

/**
 * @element umb-workspace-footer
 * @slot - Slot for workspace footer items
 * @slot actions - Slot for workspace actions
 * @class UmbWorkspaceFooterLayout
 * @augments {UmbLitElement}
 */
// TODO: stop naming this something with layout. as its not just an layout. it hooks up with extensions.
@customElement('umb-workspace-footer')
export class UmbWorkspaceFooterLayoutElement extends UmbLitElement {
	@state()
	private _modalContext?: UmbModalContext;

	@state()
	private _isNew?: boolean;

	constructor() {
		super();
		this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
			this._isNew = context?.getIsNew();
		});
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this._modalContext = context;
		});
	}

	#rejectModal = () => {
		this._modalContext?.reject();
	};

	// TODO: Some event/callback from umb-extension-slot that can be utilized to hide the footer, if empty.
	override render() {
		return html`
			<umb-footer-layout>
				<umb-extension-slot type="workspaceFooterApp"></umb-extension-slot>
				<slot></slot>
				${this._modalContext
					? html`<uui-button
							slot="actions"
							label=${this._isNew ? 'Cancel' : 'Close'}
							@click=${this.#rejectModal}></uui-button>`
					: ''}

				<slot name="actions" slot="actions"></slot>
				<umb-extension-with-api-slot
					slot="actions"
					type="workspaceAction"
					.apiArgs=${ExtensionApiArgsMethod}></umb-extension-with-api-slot>
			</umb-footer-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			/* prevents text in action buttons from wrapping */
			umb-extension-with-api-slot {
				text-wrap: nowrap;
			}

			umb-extension-slot[slot='actions'] {
				display: flex;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-footer': UmbWorkspaceFooterLayoutElement;
	}
}
