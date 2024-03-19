import { UUIIconRegistry, UUIIconRegistryElement, UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { customElement } from 'lit/decorators.js';

// @ts-ignore
import icons from '../../../Umbraco.Web.UI.Client/src/packages/core/icon-registry/icons/icons.json';

/**
 * This is a custom icon registry that will load icons from the Umbraco assets folder.
 * Optimistically, we will provide the icon, and then try and load it.
 */
class UmbIconRegistry extends UUIIconRegistry {
	protected acceptIcon(iconName: string): boolean {
    const iconManifest = icons.find((i) => i.name === iconName);
    if (!iconManifest)
      return false;
    const icon = this.provideIcon(iconName);
    const iconPath = `/umbraco/backoffice/packages/core/icon-registry/${iconManifest.path}`;
    import(/* @vite-ignore */ iconPath)
      .then((iconModule) => {
        icon.svg = iconModule.default;
      })
      .catch((err) => {
        console.error(`Failed to load icon ${iconName} on path ${iconPath}`, err.message);
      });
    return true;
	}
}

@customElement('umb-backoffice-icon-registry')
export class UmbBackofficeIconRegistryElement extends UUIIconRegistryElement {
  constructor() {
    super();
    new UUIIconRegistryEssential().attach(this);
    this.registry = new UmbIconRegistry();
  }

  protected createRenderRoot() {
    return this;
  }
}
