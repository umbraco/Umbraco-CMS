import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';
import { css, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbUserPermissionVerbElement } from '@umbraco-cms/backoffice/user';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from 'src/packages/core/lit-element/lit-element.element';
import type { MetaEntityUserPermission } from '../entity-user-permission.extension';

export abstract class UmbInputUserPermissionBaseElement<
	ManifestPermissionType extends ManifestElement & { meta: MetaEntityUserPermission },
> extends UmbFormControlMixin(UmbLitElement) {
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		if (value === this._entityType) return;
		this._entityType = value;
		this.observePermissions();
	}
	private _entityType: string = '';

	@property({ attribute: false })
	allowedVerbs: Array<string> = [];

	@state()
	protected manifests: Array<ManifestPermissionType> = [];

	protected manifestObserver?: UmbObserverController<Array<ManifestPermissionType>>;

	abstract observePermissions(): void;

	protected override getFormElement() {
		return undefined;
	}

	#isAllowed(permissionVerbs: Array<string>) {
		return permissionVerbs.every((verb) => this.allowedVerbs.includes(verb));
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

	#renderGroupedPermissions(permissionManifests: Array<ManifestPermissionType>) {
		// TODO: groupBy is not known by TS yet
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const groupedPermissions = Object.groupBy(
			permissionManifests,
			(manifest: ManifestPermissionType) => manifest.meta.group,
		) as Record<string, Array<ManifestPermissionType>>;
		return html`
			${Object.entries(groupedPermissions).map(
				([group, manifests]) => html`
					${group !== 'undefined'
						? html` <h5><umb-localize .key=${`actionCategories_${group}`}>${group}</umb-localize></h5> `
						: nothing}
					<div>${manifests.map((manifest) => html` ${this.#renderPermission(manifest)} `)}</div>
				`,
			)}
		`;
	}

	#renderPermission(manifest: ManifestPermissionType) {
		return html` <umb-input-user-permission-verb
			label=${ifDefined(manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name)}
			description=${ifDefined(manifest.meta.description ? this.localize.string(manifest.meta.description) : undefined)}
			?allowed=${this.#isAllowed(manifest.meta.verbs)}
			@change=${(event: UmbChangeEvent) =>
				this.#onChangeUserPermission(event, manifest.meta.verbs)}></umb-input-user-permission-verb>`;
	}

	override render() {
		return html`${this.#renderGroupedPermissions(this.manifests)} `;
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.manifestObserver?.destroy();
	}

	static override styles = css`
		umb-input-user-permission-verb:not(:last-of-type) {
			border-bottom: 1px solid var(--uui-color-divider);
		}
	`;
}
