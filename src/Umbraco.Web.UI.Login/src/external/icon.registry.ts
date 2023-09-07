import { UUIIconRegistry, UUIIconRegistryElement, UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { Observable, ReplaySubject } from 'rxjs';
import { customElement } from 'lit/decorators.js';

/**
 * This is a custom icon registry that will load icons from the Umbraco assets folder.
 * Optimistically, we will provide the icon, and then try and load it.
 */
class UmbIconRegistry extends UUIIconRegistry {
	protected acceptIcon(iconName: string): boolean {
    // If the icon name is a variable, we will not provide it.
    if (iconName.startsWith('{{') && iconName.endsWith('}}')) {
      return false;
    }

		// Inform that we will be providing this.
		const icon = this.provideIcon(iconName);

		this.#getIcons().subscribe((icons) => {
			if (icons[iconName]) {
				icon.svg = icons[iconName];
			} else {
				// If we can't load the icon, we will not provide it.
				console.warn(`Icon ${iconName} not found`);
			}
		});

		return true;
	}

  #iconsLoaded = false;
  #icons = new ReplaySubject<Record<string, string>>(1);
  #getIcons(): Observable<Record<string, string>> {
    if (!this.#iconsLoaded) {
      this.#iconsLoaded = true;
      fetch('backoffice/umbracoapi/icon/geticons')
        .then((response) => {
          if (!response.ok) {
            throw new Error('Could not fetch icons');
          }

          return response.json();
        })
        .then((icons) => {
          this.#icons.next(icons);
          this.#icons.complete();
        });
    }

    return this.#icons.asObservable();
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
