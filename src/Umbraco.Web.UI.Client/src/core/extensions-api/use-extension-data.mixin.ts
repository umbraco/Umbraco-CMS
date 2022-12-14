import type { HTMLElementConstructor } from '../models';

export declare class UmbUseExtensionDataMixinInterface {
	extensionMeta: object;
}

export const UmbUseExtensionDataMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbUseExtensionDataClass extends superClass {
		extensionMeta: object = {};
	}

	return UmbUseExtensionDataClass as unknown as HTMLElementConstructor<UmbUseExtensionDataMixinInterface> & T;
};
