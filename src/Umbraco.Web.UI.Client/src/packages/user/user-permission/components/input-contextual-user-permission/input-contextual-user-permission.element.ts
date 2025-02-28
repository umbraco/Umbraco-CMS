import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../../../user-group/constants.js';
import type { ManifestContextualUserPermission, UmbContextualUserPermissionModel } from '../../types.js';
import type { UmbUserPermissionVerbElement } from '../index.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-contextual-user-permission')
export class UmbInputContextualUserPermissionElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	private _manifest?: ManifestContextualUserPermission | undefined;
	public get manifest(): ManifestContextualUserPermission | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestContextualUserPermission | undefined) {
		this._manifest = value;

		if (!this._manifest) {
			this._label = undefined;
			this._description = undefined;
			this._permission = undefined;
			return;
		}

		this._label = this._manifest.meta.label ? this.localize.string(this._manifest.meta.label) : this._manifest.name;
		this._description = this.manifest?.meta.description
			? this.localize.string(this.manifest.meta.description)
			: undefined;

		this._permission = {
			$type: 'UnknownTypePermissionPresentationModel',
			context: this._manifest.meta.permission.context,
			verbs: this._manifest.meta.permission.verbs,
		};
	}

	@state()
	_label?: string;

	@state()
	_description?: string;

	@state()
	_userGroupPermissions: Array<UmbContextualUserPermissionModel> = [];

	@state()
	_permission?: UmbContextualUserPermissionModel;

	#context?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(
				this.#context.permissions,
				(permissions) => (this._userGroupPermissions = permissions),
				'umbPermissionsObserver',
			);
		});
	}

	#onChangeUserPermission(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbUserPermissionVerbElement;
		if (target.allowed) {
			this.#add();
		} else {
			this.#remove();
		}
	}

	#add() {
		if (!this._permission) {
			throw new Error('Permission is not set');
		}

		this.#context?.addContextualPermission(this._permission);
	}

	#remove() {
		if (!this._permission) {
			throw new Error('Permission is not set');
		}

		this.#context?.removeContextualPermission(this._permission);
	}

	#isAllowed() {
		const contextualPermissions = this._userGroupPermissions.filter(
			(permission) =>
				permission.$type === 'UnknownTypePermissionPresentationModel' &&
				permission.context === this._permission?.context,
		);

		let isAllowed = false;

		contextualPermissions.forEach((permission) => {
			// ensure that all verbs in this permission is stored on the user group
			if (this._permission?.verbs.every((verb) => permission?.verbs.includes(verb))) {
				isAllowed = true;
			}
		});

		return isAllowed;
	}

	override render() {
		return html` <umb-input-user-permission-verb
			label=${ifDefined(this._label)}
			description=${ifDefined(this._description)}
			?allowed=${this.#isAllowed()}
			@change=${(event: UmbChangeEvent) => this.#onChangeUserPermission(event)}></umb-input-user-permission-verb>`;
	}
}

export { UmbInputContextualUserPermissionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-contextual-user-permission': UmbInputContextualUserPermissionElement;
	}
}
