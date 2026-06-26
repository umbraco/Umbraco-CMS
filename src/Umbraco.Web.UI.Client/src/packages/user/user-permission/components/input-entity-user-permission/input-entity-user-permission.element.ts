import type { ManifestEntityUserPermission } from '../../entity-user-permission.extension.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state, ifDefined, css, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbUserPermissionVerbElement } from '@umbraco-cms/backoffice/user';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-entity-user-permission')
export class UmbInputEntityUserPermissionElement extends UmbFormControlMixin(UmbLitElement) {
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		if (value === this._entityType) return;
		this._entityType = value;
		this.#observeEntityUserPermissions();
	}
	private _entityType: string = '';

	@property({ attribute: false })
	allowedVerbs: Array<string> = [];

	@state()
	private _groupedPermissions: Array<[string, ManifestEntityUserPermission[]]> = [];

	#manifestObserver?: UmbObserverController<Array<ManifestEntityUserPermission>>;

	protected override getFormElement() {
		return undefined;
	}

	#isAllowed(permissionVerbs: Array<string>) {
		return permissionVerbs.every((verb) => this.allowedVerbs.includes(verb));
	}

	#observeEntityUserPermissions() {
		this.#manifestObserver?.destroy();

		this.#manifestObserver = this.observe(
			umbExtensionsRegistry.byTypeAndFilter('entityUserPermission', (manifest) =>
				manifest.forEntityTypes.includes(this.entityType),
			),
			(manifests) => {
				// TODO: groupBy is not known by TS yet
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-expect-error
				const groupedPermissions = Object.groupBy(
					manifests,
					(manifest: ManifestEntityUserPermission) => manifest.meta.group,
				) as Record<string, Array<ManifestEntityUserPermission>>;
				this._groupedPermissions = Object.entries(groupedPermissions);
			},
			'umbUserPermissionManifestsObserver',
		);
	}

	#onChangeUserPermission(event: UmbChangeEvent, permissionVerbs: Array<string>) {
		event.stopPropagation();
		const target = event.target as UmbUserPermissionVerbElement;
		if (target.allowed) {
			this.#addUserPermission(permissionVerbs);
		} else {
			this.#removeUserPermission(permissionVerbs);
		}
	}

	#addUserPermission(permissionVerbs: Array<string>) {
		const verbs = [...this.allowedVerbs, ...permissionVerbs];
		// ensure we only have unique verbs
		this.allowedVerbs = [...new Set(verbs)];
		this.dispatchEvent(new UmbChangeEvent());
	}

	#removeUserPermission(permissionVerbs: Array<string>) {
		this.allowedVerbs = this.allowedVerbs.filter((p) => !permissionVerbs.includes(p));
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return repeat(this._groupedPermissions, ([group, manifests]) => {
			const headline = group !== 'undefined' ? `#actionCategories_${group}` : `#actionCategories_general`;
			return html`
				<umb-property-layout label=${headline}>
					<div slot="editor">${repeat(manifests, (manifest) => html` ${this.#renderPermission(manifest)} `)}</div>
				</umb-property-layout>
			`;
		});
	}

	#renderPermission(manifest: ManifestEntityUserPermission) {
		return html` <umb-input-user-permission-verb
			label=${ifDefined(manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name)}
			description=${ifDefined(manifest.meta.description ? this.localize.string(manifest.meta.description) : undefined)}
			?allowed=${this.#isAllowed(manifest.meta.verbs)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, manifest.meta.verbs)}></umb-input-user-permission-verb>`;
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#manifestObserver?.destroy();
	}

	static override styles = css`
		umb-input-user-permission-verb:not(:last-of-type) {
			border-bottom: 1px solid var(--uui-color-divider);
		}
	`;
}

export default UmbInputEntityUserPermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-entity-user-permission': UmbInputEntityUserPermissionElement;
	}
}
