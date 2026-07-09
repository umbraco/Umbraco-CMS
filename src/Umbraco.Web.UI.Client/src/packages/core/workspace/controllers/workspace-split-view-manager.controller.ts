import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_WORKSPACE_VARIANT_DELIMITER } from './constants.js';

export type UmbActiveVariant = {
	index: number;
	culture: string | null;
	segment: string | null;
};

/**
 * @deprecated Use {@link UmbActiveVariant} instead. This will be removed in Umbraco 18.
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export type ActiveVariant = UmbActiveVariant;

/**
 * @class UmbWorkspaceSplitViewManager
 * @description - Class managing the split view state for a workspace context.
 */
export class UmbWorkspaceSplitViewManager {
	#activeVariantsInfo = new UmbArrayState<UmbActiveVariant>([], (x) => x.index).sortBy(
		(a, b) => (a.index || 0) - (b.index || 0),
	);
	public readonly activeVariantsInfo = this.#activeVariantsInfo.asObservable();
	public readonly firstActiveVariantInfo = this.#activeVariantsInfo.asObservablePart((x) => x[0]);
	public readonly splitViewActive = this.#activeVariantsInfo.asObservablePart((x) => x.length > 1);

	private _routeBase?: string;
	public getWorkspaceRoute(): string | undefined {
		return this._routeBase;
	}
	public setWorkspaceRoute(route: string | undefined) {
		this._routeBase = route;
	}

	setActiveVariant(index: number, culture: string | null, segment: string | null) {
		this.#activeVariantsInfo.appendOneAt({ index, culture: culture ?? null, segment: segment ?? null }, index);
	}

	getActiveVariants() {
		return this.#activeVariantsInfo.getValue();
	}

	public removeActiveVariant(index: number) {
		if (this.getActiveVariants().length > 1) {
			this.#activeVariantsInfo.removeOne(index);
		}
	}

	public activeVariantByIndex(index: number) {
		return this.#activeVariantsInfo.asObservablePart((data) => data.find((x) => x.index === index) || undefined);
	}

	public openVariants(variants: Array<UmbVariantId>) {
		const active = this.getActiveVariants();
		this.#activeVariantsInfo.mute();
		active.forEach((v, index) => {
			if (index >= variants.length) {
				this.removeActiveVariant(v.index);
			}
		});
		variants.forEach((variant, index) => {
			this.setActiveVariant(index, variant.culture, variant.segment);
		});
		this.#activeVariantsInfo.unmute();
	}

	public switchVariant(index: number, variantId: UmbVariantId) {
		// TODO: remember current path and extend url with it.
		// TODO: construct URl with all active routes:
		// TODO: use method for generating variant url:
		const workspaceRoute = this.getWorkspaceRoute();
		if (workspaceRoute) {
			const activeVariants = this.getActiveVariants();
			if (activeVariants && index < activeVariants.length) {
				const newVariants = [...activeVariants];
				newVariants[index] = { index, culture: variantId.culture, segment: variantId.segment };

				const variantPart: string = newVariants
					.map((v) => UmbVariantId.Create(v).toString())
					.join(UMB_WORKSPACE_VARIANT_DELIMITER);

				const additionalPathname = this.#getAdditionalPathname();
				history.pushState(null, '', `${workspaceRoute}/${variantPart}${additionalPathname}`);
				return true;
			}
		}
		return false;
	}

	public openSplitView(newVariant: UmbVariantId) {
		// TODO: remember current path and extend url with it.
		// TODO: construct URl with all active routes:
		// TODO: use method for generating variant url:

		const currentVariant = this.getActiveVariants()[0];
		const workspaceRoute = this.getWorkspaceRoute();
		if (currentVariant && workspaceRoute) {
			const currentVariantId = UmbVariantId.Create(currentVariant);
			history.pushState(
				null,
				'',
				`${workspaceRoute}/${currentVariantId}${UMB_WORKSPACE_VARIANT_DELIMITER}${newVariant}`,
			);
			return true;
		}
		return false;
	}

	public closeSplitView(index: number) {
		const workspaceRoute = this.getWorkspaceRoute();
		if (workspaceRoute) {
			const activeVariants = this.getActiveVariants();
			if (activeVariants && index < activeVariants.length) {
				const newVariants = activeVariants.filter((x) => x.index !== index);

				const variantPart: string = newVariants
					.map((v) => UmbVariantId.Create(v))
					.join(UMB_WORKSPACE_VARIANT_DELIMITER);

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
				return true;
			}
		}
		return false;
	}

	public setVariantParts(routeFragment: string) {
		const variantSplit = routeFragment.split(UMB_WORKSPACE_VARIANT_DELIMITER);
		this.openVariants(variantSplit.map((v) => UmbVariantId.FromString(v)));
	}

	/**
	 * @deprecated Use {@link openVariants} instead. This will be removed in Umbraco 20.
	 */
	public handleVariantFolderPart(index: number, folderPart: string) {
		new UmbDeprecation({
			deprecated: 'UmbWorkspaceSplitViewManager .handleVariantFolderPart method.',
			removeInVersion: '20',
			solution: 'Use the .openVariants method instead.',
		}).warn();
		const variantId = UmbVariantId.FromString(folderPart);
		this.setActiveVariant(index, variantId.culture, variantId.segment);
	}

	#getCurrentVariantPathname() {
		const workspaceRoute = this.getWorkspaceRoute();
		const activeVariants = this.getActiveVariants();
		const currentVariantPart: string = activeVariants
			.map((v) => UmbVariantId.Create(v).toString())
			.join(UMB_WORKSPACE_VARIANT_DELIMITER);

		return `${workspaceRoute}/${currentVariantPart}`;
	}

	#getAdditionalPathname() {
		const currentUrl = new URL(window.location.href);
		const currentFullPathname = currentUrl.pathname;
		const currentVariantPathname = this.#getCurrentVariantPathname();

		if (currentVariantPathname && currentFullPathname.startsWith(currentVariantPathname)) {
			return currentFullPathname.substring(currentVariantPathname.length);
		}

		// If the currentVariantPathname is not a prefix, return empty string
		return '';
	}
}
