import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository } from '@umbraco-cms/backoffice/media';

/**
 *  @element umb-user-group-ref
 *  @description - Component for displaying a reference to a User Group
 *  @augments UUIRefNodeElement
 */
@customElement('umb-user-group-ref')
export class UmbUserGroupRefElement extends UmbElementMixin(UUIRefNodeElement) {
	#documentItemRepository?: UmbDocumentItemRepository;
	#mediaItemRepository?: UmbMediaItemRepository;

	@property({ type: Boolean })
	documentRootAccess: boolean = false;

	@property()
	public get documentStartNode(): string | null | undefined {
		return '';
	}
	public set documentStartNode(value: string | null | undefined) {
		this.#observeDocumentStartNode(value);
	}

	@property({ type: Boolean })
	mediaRootAccess: boolean = false;

	@property()
	public get mediaStartNode(): string | null | undefined {
		return '';
	}
	public set mediaStartNode(value: string | null | undefined) {
		this.#observeMediaStartNode(value);
	}

	@property({ type: Array })
	public get sections(): Array<string> {
		return [];
	}
	public set sections(value: Array<string>) {
		this.#observeSections(value);
	}

	@property({ type: Array, attribute: 'user-permission-aliases' })
	public get userPermissionAliases(): Array<string> {
		return [];
	}
	public set userPermissionAliases(value: Array<string>) {
		this.#observeUserPermissions(value);
	}

	@state()
	private _documentLabel: string = '';

	@state()
	private _mediaLabel: string = '';

	@state()
	private _sectionLabels: Array<string> = [];

	@state()
	private _userPermissionLabels: Array<string> = [];

	async #observeDocumentStartNode(unique: string | null | undefined) {
		if (this.documentRootAccess) return;
		if (!unique) return;

		if (!this.#documentItemRepository) {
			this.#documentItemRepository = new UmbDocumentItemRepository(this);
		}

		const { error, asObservable } = await this.#documentItemRepository.requestItems([unique]);
		if (error) return;

		this.observe(
			asObservable(),
			(data) => (this._documentLabel = data[0].variants?.[0].name ?? unique),
			'_observeDocumentStartNode',
		);
	}

	async #observeMediaStartNode(unique: string | null | undefined) {
		if (this.mediaRootAccess) return;
		if (!unique) return;

		if (!this.#mediaItemRepository) {
			this.#mediaItemRepository = new UmbMediaItemRepository(this);
		}

		const { error, asObservable } = await this.#mediaItemRepository.requestItems([unique]);
		if (error) return;

		this.observe(
			asObservable(),
			(data) => (this._mediaLabel = data[0].variants?.[0].name ?? unique),
			'_observeMediaStartNode',
		);
	}

	async #observeSections(aliases: Array<string>) {
		if (aliases?.length) {
			this.observe(
				umbExtensionsRegistry.byTypeAndAliases('section', aliases),
				(manifests) => {
					this._sectionLabels = manifests.map((manifest) =>
						manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name,
					);
				},
				'_observeSections',
			);
		} else {
			this.removeUmbControllerByAlias('_observeSections');
		}
	}

	async #observeUserPermissions(aliases: Array<string>) {
		if (aliases?.length) {
			this.observe(
				umbExtensionsRegistry.byTypeAndAliases('entityUserPermission', aliases),
				(manifests) => {
					this._userPermissionLabels = manifests.map((manifest) =>
						manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name,
					);
				},
				'_observeUserPermission',
			);
		} else {
			this.removeUmbControllerByAlias('_observeUserPermission');
		}
	}

	protected override renderDetail() {
		return html`
			<small id="detail">${this.detail}</small>
			${this.#renderDetails()}
			<slot name="detail"></slot>
		`;
	}

	#renderDetails() {
		const hasSections = this._sectionLabels.length;
		const hasDocument = !!this._documentLabel || this.documentRootAccess;
		const hasMedia = !!this._mediaLabel || this.mediaRootAccess;
		const hasUserPermissions = this._userPermissionLabels.length;

		if (!hasSections && !hasDocument && !hasMedia && !hasUserPermissions) return;

		return html`
			<div id="details">
				${this.#renderSections()} ${this.#renderDocumentStartNode()} ${this.#renderMediaStartNode()}
				${this.#renderUserPermissions()}
			</div>
		`;
	}

	#renderSections() {
		if (!this._sectionLabels.length) return;
		return html`
			<div>
				<small>
					<strong><umb-localize key="main_sections">Sections</umb-localize>:</strong>
					${this._sectionLabels.join(', ')}
				</small>
			</div>
		`;
	}

	#renderDocumentStartNode() {
		if (!this._documentLabel && !this.documentRootAccess) return;
		return html`
			<div>
				<small>
					<strong><umb-localize key="user_startnode">Document access</umb-localize>:</strong>
					${this.documentRootAccess ? this.localize.term('contentTypeEditor_allDocuments') : this._documentLabel}
				</small>
			</div>
		`;
	}

	#renderMediaStartNode() {
		if (!this._mediaLabel && !this.mediaRootAccess) return;
		return html`
			<div>
				<small>
					<strong><umb-localize key="user_mediastartnode">Media access</umb-localize>:</strong>
					${this.mediaRootAccess ? this.localize.term('contentTypeEditor_allMediaItems') : this._mediaLabel}
				</small>
			</div>
		`;
	}

	#renderUserPermissions() {
		if (!this._userPermissionLabels.length) return;
		return html`
			<div>
				<small>
					<strong><umb-localize key="user_userPermissions">User permissions</umb-localize>:</strong>
					${this._userPermissionLabels.join(', ')}
				</small>
			</div>
		`;
	}

	static override styles = [
		...UUIRefNodeElement.styles,
		css`
			#details {
				color: var(--uui-color-text-alt);
				margin-top: var(--uui-size-space-1);
			}

			#details > div {
				margin-bottom: var(--uui-size-space-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-ref': UmbUserGroupRefElement;
	}
}
