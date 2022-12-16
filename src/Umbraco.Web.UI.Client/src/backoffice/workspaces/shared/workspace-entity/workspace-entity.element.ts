import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';

import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestWorkspace } from '@umbraco-cms/models';

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	@property()
	public entityKey!: string;

	private _entityType = '';
	@property()
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		this._entityType = value;
		this._observeWorkspace();
	}

	@state()
	private _element?: HTMLElement;

	private _currentWorkspaceAlias:string | null = null;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeWorkspace();
	}

	/**
	TODO: use future system of extension-slot, extension slots must use a condition-system which will be used to determine the filtering happening below.
	This will first be possible to make when ContextApi is available, as conditions will use this system.
	*/
	private _observeWorkspace() {
		this.observe<ManifestWorkspace | undefined>(
			umbExtensionsRegistry
				.extensionsOfType('workspace')
				.pipe(map((workspaces) => workspaces.find((workspace) => workspace.meta.entityType === this.entityType))),
			(workspace) => {
				// don't rerender workspace if it's the same
				const newWorkspaceAlias = workspace?.alias || '';
				if (this._currentWorkspaceAlias === newWorkspaceAlias) return;
				this._currentWorkspaceAlias = newWorkspaceAlias;
				this._createElement(workspace);
			}
		);
	}

	private async _createElement(workspace?: ManifestWorkspace) {
		this._element = workspace ? (await createExtensionElement(workspace)) : undefined;
		if (this._element) {
			// TODO: use contextApi for this.
			(this._element as any).entityKey = this.entityKey;
			return;
		}

		// TODO: implement fallback workspace
		// Note for extension-slot, we must enable giving the extension-slot a fallback element.
		const fallbackWorkspace = document.createElement('div');
		fallbackWorkspace.innerHTML = '<p>No Workspace found</p>';
		this._element = fallbackWorkspace;
	}

	render() {
		return html`${this._element}`;
	}
}

export default UmbWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
