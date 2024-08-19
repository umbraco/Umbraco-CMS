import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export type ActiveVariant = {
	index: number;
	culture: string | null;
	segment: string | null;
};

/**
 * @class UmbWorkspaceSplitViewManager
 * @description - Class managing the split view state for a workspace context.
 */
export class UmbWorkspaceSplitViewManager {
	#activeVariantsInfo = new UmbArrayState<ActiveVariant>([], (x) => x.index).sortBy(
		(a, b) => (a.index || 0) - (b.index || 0),
	);
	public readonly activeVariantsInfo = this.#activeVariantsInfo.asObservable();

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

				const variantPart: string = newVariants.map((v) => UmbVariantId.Create(v).toString()).join('_&_');

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
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
			history.pushState(null, '', `${workspaceRoute}/${UmbVariantId.Create(currentVariant)}_&_${newVariant}`);
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

				const variantPart: string = newVariants.map((v) => UmbVariantId.Create(v)).join('_&_');

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
				return true;
			}
		}
		return false;
	}
}
