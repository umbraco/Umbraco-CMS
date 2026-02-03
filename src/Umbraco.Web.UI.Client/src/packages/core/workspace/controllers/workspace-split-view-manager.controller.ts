import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

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

const UBM_VARIANT_DELIMITER = '_&_';

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
					.join(UBM_VARIANT_DELIMITER);

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
			history.pushState(
				null,
				'',
				`${workspaceRoute}/${UmbVariantId.Create(currentVariant)}${UBM_VARIANT_DELIMITER}${newVariant}`,
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

				const variantPart: string = newVariants.map((v) => UmbVariantId.Create(v)).join(UBM_VARIANT_DELIMITER);

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
				return true;
			}
		}
		return false;
	}

	public setVariantParts(routeFragment: string) {
		const variantSplit = routeFragment.split(UBM_VARIANT_DELIMITER);
		variantSplit.forEach((part, index) => {
			this.handleVariantFolderPart(index, part);
		});
	}

	public handleVariantFolderPart(index: number, folderPart: string) {
		const variantId = UmbVariantId.FromString(folderPart);
		this.setActiveVariant(index, variantId.culture, variantId.segment);
	}

	#getCurrentVariantPathname() {
		const workspaceRoute = this.getWorkspaceRoute();
		const activeVariants = this.getActiveVariants();
		const currentVariantPart: string = activeVariants
			.map((v) => UmbVariantId.Create(v).toString())
			.join(UBM_VARIANT_DELIMITER);

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
