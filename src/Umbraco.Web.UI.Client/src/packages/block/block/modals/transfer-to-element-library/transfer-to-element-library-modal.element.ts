import {
	umbElementPermissionVerbsSatisfy,
	umbResolveElementPermissionVerbs,
} from '../../utils/element-permission-verbs.function.js';
import type {
	UmbBlockTransferToElementLibraryModalData,
	UmbBlockTransferToElementLibraryModalValue,
} from './transfer-to-element-library-modal.token.js';
import { css, customElement, html, ifDefined, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import {
	UMB_ELEMENT_FOLDER_ENTITY_TYPE,
	UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_CREATE,
	UMB_USER_PERMISSION_ELEMENT_PUBLISH,
	UmbElementTypeStructureRepository,
} from '@umbraco-cms/backoffice/element';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

@customElement('umb-block-transfer-to-element-library-modal')
export class UmbBlockTransferToElementLibraryModalElement extends UmbModalBaseElement<
	UmbBlockTransferToElementLibraryModalData,
	UmbBlockTransferToElementLibraryModalValue
> {
	@state()
	private _name = '';

	@state()
	private _parentUnique: string | null = null;

	@state()
	private _hasSelectedLocation = false;

	@state()
	private _isCheckingLocation = false;

	@state()
	private _locationError?: string;

	#locationRequest = 0;

	override connectedCallback() {
		super.connectedCallback();
		if (this.data?.name) {
			this._name = this.data.name;
		}
	}

	#onNameInput(e: UUIInputEvent) {
		this._name = e.target.value as string;
	}

	#onFolderSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._parentUnique = event.unique ?? null;
		this._hasSelectedLocation = true;
		this.#checkLocation(this._parentUnique);
	}

	#onFolderDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._parentUnique = null;
		this._hasSelectedLocation = false;
		this.#locationRequest++;
		this._isCheckingLocation = false;
		this._locationError = undefined;
	}

	async #checkLocation(parentUnique: string | null) {
		const request = ++this.#locationRequest;
		this._isCheckingLocation = true;
		this._locationError = undefined;

		let error: string | undefined;
		try {
			error = await this.#getLocationError(parentUnique);
		} catch {
			error = this.localize.term('blockEditor_transferToElementLibraryLocationNotPermitted');
		}

		// Ignore the outcome when another location was chosen in the meantime.
		if (request !== this.#locationRequest) return;
		this._locationError = error;
		this._isCheckingLocation = false;
	}

	async #getLocationError(parentUnique: string | null): Promise<string | undefined> {
		if (!(await this.#isCreateAndPublishPermittedAt(parentUnique))) {
			return this.localize.term('blockEditor_transferToElementLibraryLocationNotPermitted');
		}
		if (!(await this.#isElementTypeAllowedAt(parentUnique))) {
			return this.localize.term('blockEditor_transferToElementLibraryLocationTypeNotAllowed');
		}
		return undefined;
	}

	async #isCreateAndPublishPermittedAt(parentUnique: string | null): Promise<boolean> {
		const context = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		if (!context) return false;
		const user = await this.observe(context.currentUser).asPromise();

		const verbs = umbResolveElementPermissionVerbs({
			permissions: user.permissions,
			fallbackPermissions: user.fallbackPermissions,
			path: await this.#getPathTo(parentUnique),
		});

		return umbElementPermissionVerbsSatisfy(verbs, {
			allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE, UMB_USER_PERMISSION_ELEMENT_PUBLISH],
		});
	}

	async #getPathTo(parentUnique: string | null): Promise<Array<string>> {
		if (parentUnique === null) return [];

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<UmbTreeItemModel, UmbTreeRootModel>>(
			this,
			UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
		);
		const { data: ancestors } = await treeRepository.requestTreeItemAncestors({
			treeItem: { unique: parentUnique, entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE },
		});

		return [...(ancestors?.map((ancestor) => ancestor.unique) ?? []), parentUnique];
	}

	async #isElementTypeAllowedAt(parentUnique: string | null): Promise<boolean> {
		const contentTypeKey = this.data?.contentTypeKey;
		if (!contentTypeKey) return true;

		const { data } = await new UmbElementTypeStructureRepository(this).requestAllowedChildrenOf(null, parentUnique);
		return data?.items.some((item) => item.unique === contentTypeKey) ?? false;
	}

	#onTransfer() {
		this.value = { name: this._name, parentUnique: this._parentUnique };
		this.modalContext?.submit();
	}

	override render() {
		const treeProps = { hideTreeItemActions: true, foldersOnly: true };
		const canTransfer =
			!!this._name.trim() && this._hasSelectedLocation && !this._isCheckingLocation && !this._locationError;
		return html`
			<umb-body-layout headline=${this.localize.term('blockEditor_transferToElementLibrary')}>
				<uui-box>
					<umb-property-layout label="#general_name" orientation="vertical" mandatory>
						<uui-input
							slot="editor"
							required
							label=${this.localize.term('general_name')}
							.value=${this._name}
							@input=${this.#onNameInput}
							${umbFocus()}>
						</uui-input>
					</umb-property-layout>
					<umb-property-layout label="#general_choose" orientation="vertical" mandatory>
						<div slot="editor">
							<umb-tree
								alias="Umb.Tree.Element"
								.props=${treeProps}
								@selected=${this.#onFolderSelected}
								@deselected=${this.#onFolderDeselected}>
							</umb-tree>
							<umb-form-validation-message>
								${when(this._locationError, () => html`<span slot="message">${this._locationError}</span>`)}
							</umb-form-validation-message>
						</div>
					</umb-property-layout>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					label=${this.localize.term('blockEditor_transferToElementLibrary')}
					look="primary"
					color="positive"
					state=${ifDefined(this._isCheckingLocation ? 'waiting' : undefined)}
					?disabled=${!canTransfer}
					@click=${this.#onTransfer}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbBlockTransferToElementLibraryModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-transfer-to-element-library-modal': UmbBlockTransferToElementLibraryModalElement;
	}
}
