import type { ManifestWorkspace } from './extensions/types.js';
import { UmbDefaultWorkspaceContext } from './kinds/default/default-workspace.context.js';
import { customElement, property, state, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbParallelHookController } from '@umbraco-cms/backoffice/hook-api';
import {
	UmbExtensionsApiInitializer,
	UmbExtensionsElementAndApiInitializer,
	type UmbApiConstructorArgumentsMethodType,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
const WORKSPACE_CONTEXTS_SETTLE_TIMEOUT = 2000;

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementAndApiInitializer<any>;
	#entityType?: string;

	@state()
	private _component?: HTMLElement;

	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string | undefined {
		return this.#entityType;
	}
	public set entityType(value: string) {
		if (value === this.#entityType || !value) return;
		this.#entityType = value;
		this.#createController(value);
	}

	#createController(entityType: string) {
		if (this.#extensionsController) {
			this.#extensionsController.destroy();
		}
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'workspace:' + entityType);
		this.#extensionsController = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'workspace',
			apiArgsCreator,
			(manifest: ManifestWorkspace) => manifest.meta.entityType === entityType,
			(extensionControllers) => {
				this._component = extensionControllers[0]?.component;
				const api = extensionControllers[0]?.api;
				if (api) {
					// We create the additional workspace contexts with the Workspace API as its host, to ensure they can use the same Context-Alias with different API-Aliases and still be reached cause they will then be provided at the same host. [NL]
					const workspaceContexts = new UmbExtensionsApiInitializer(api, umbExtensionsRegistry, 'workspaceContext', [
						api,
					]);
					// Let a workspace that loads entities wait for its additional workspace contexts to be ready, before processing the incoming data. [NL]
					if ('loadingHook' in api && api.loadingHook instanceof UmbParallelHookController) {
						api.loadingHook.add(async () => {
							let timer: ReturnType<typeof setTimeout> | undefined;
							const timeout = new Promise<void>((resolve) => {
								timer = setTimeout(() => {
									console.warn(
										`Workspace contexts for "${entityType}" did not all settle within ${WORKSPACE_CONTEXTS_SETTLE_TIMEOUT}ms, continuing without waiting any longer. Still waiting for:`,
										workspaceContexts.getUnsettledAliases(),
									);
									resolve();
								}, WORKSPACE_CONTEXTS_SETTLE_TIMEOUT);
							});
							try {
								await Promise.race([workspaceContexts.asSettledPromise(), timeout]);
							} finally {
								clearTimeout(timer);
							}
						});
					}
				}
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
			undefined,
			UmbDefaultWorkspaceContext,
			{ single: true },
		);
	}

	override render() {
		return this._component ?? html`<umb-view-loader></umb-view-loader>`;
	}
}

export { UmbWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
