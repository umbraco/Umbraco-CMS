import {UUIIconRegistry} from "@umbraco-ui/uui";

/**
 * This is a custom icon registry that will load icons from the Umbraco assets folder.
 * Optimistically, we will provide the icon, and then try and load it.
 */
export class UmbIconRegistry extends UUIIconRegistry {
  protected acceptIcon(iconName: string): boolean {
    // Inform that we will be providing this.
    const icon = this.provideIcon(iconName);

    // Try and load icon
    const iconPath = `assets/icons/${iconName}.svg`;
    fetch(iconPath)
      .then(async response => {
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }

        return response.text();
      })
      .then(svg => {
        icon.svg = svg;
      })
      .catch(() => {
        // If we can't load the icon, we will not provide it.
        console.warn(`Icon ${iconName} not found`);
      });
    return true;
  }
}
