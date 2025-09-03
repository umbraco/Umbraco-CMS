import type { ManifestWorkspaceView } from '../../types.js';
import { UmbWorkspaceViewContext } from './workspace-view.context.js';
import { UMB_WORKSPACE_EDITOR_CONTEXT } from './workspace-editor.context-token.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBasicState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbHintController } from '@umbraco-cms/backoffice/hint';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbWorkspaceEditorContext extends UmbContextBase {
	//
	#init: Promise<void>;
	/**
	 * State holding the permitted Workspace Views as a Workspace View Context
	 */

	#manifests = new UmbBasicState<Array<ManifestWorkspaceView>>([]);
	#overrides = new UmbBasicState<Array<UmbDeepPartialObject<ManifestWorkspaceView>>>([]);
	public readonly views = mergeObservables(
		[this.#manifests.asObservable(), this.#overrides.asObservable()],
		([manifests, overrides]): Array<UmbWorkspaceViewContext> => {
			let contexts = this.#contexts;

			// remove ones that are no longer contained in the workspaceViews (And thereby make the new array):
			const contextsToKeep = contexts.filter(
				(view) => !manifests.some((manifest) => manifest.alias === view.manifest.alias),
			);

			const hasDiff = contextsToKeep.length !== manifests.length;
			if (hasDiff) {
				contexts = [...contextsToKeep];

				// Add ones that are new:
				manifests
					.filter((manifest) => !contextsToKeep.some((x) => x.manifest.alias === manifest.alias))
					.forEach((manifest) => {
						const context = new UmbWorkspaceViewContext(this, manifest);
						context.setVariantId(this.#variantId);
						context.hints.inheritFrom(this.#hints);
						contexts.push(context);
					});
			}

			// Apply overrides:
			contexts.forEach((context) => {
				const override = overrides.find((x) => x.alias === context.manifest.alias);
				if (override) {
					// Test to see if there is a change, to avoid unnecessary updates, this prevents re-setting the manifest again and again. [NL]
					const overrideKeys = Object.keys(override) as Array<keyof ManifestWorkspaceView>;
					const hasOverrideDiff = overrideKeys.some((key) => context.manifest[key] !== override[key]);
					if (hasOverrideDiff) {
						context.manifest = {
							...context.manifest,
							...(override as ManifestWorkspaceView),
							meta: { ...context.manifest.meta, ...override.meta },
						};
					}
				}
			});

			// sort contexts to match manifests weights:
			contexts.sort((a, b): number => (b.manifest.weight || 0) - (a.manifest.weight || 0));

			this.#contexts = contexts;
			return contexts;
		},
		// Custom memoize method, to check context instance and manifest instance:
		(previousValue: Array<UmbWorkspaceViewContext>, currentValue: Array<UmbWorkspaceViewContext>): boolean => {
			return (
				previousValue === currentValue &&
				currentValue.some(
					(x) => x.manifest === previousValue.find((y) => y.manifest.alias === x.manifest.alias)?.manifest,
				)
			);
		},
	);
	// A storage and cache for the current contexts, to enable communicating to them and to avoid re-initializing them every time there is a change of manifests/overrides. [NL]
	#contexts = new Array<UmbWorkspaceViewContext>();

	#variantId?: UmbVariantId;
	#hints = new UmbHintController<UmbVariantHint>(this, {});

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_EDITOR_CONTEXT);

		this.#hints.inherit();

		this.#init = new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'workspaceView',
			null,
			(workspaceViews) => {
				this.#manifests.setValue(workspaceViews.map((controller) => controller.manifest));
			},
		).asPromise();
	}

	setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId = variantId;
		this.#hints.updateScaffold({ variantId });
		this.#contexts.forEach((view) => {
			view.hints.updateScaffold({ variantId });
		});
	}

	setOverrides(overrides?: Array<UmbDeepPartialObject<ManifestWorkspaceView>>): void {
		this.#overrides.setValue(overrides ?? []);
	}

	async getViewContext(alias: string): Promise<UmbWorkspaceViewContext | undefined> {
		await this.#init;
		return this.#contexts.find((view) => view.manifest.alias === alias);
	}
}
