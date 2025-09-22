# Images Directory

This directory contains static images and icons for the Vehicle Maintenance Log application.

## Structure:
- `/images/` - Main application images and logos
- `/images/icons/` - Custom SVG icons for features
- `/images/backgrounds/` - Background images (create as needed)
- `/images/avatars/` - User avatars (create as needed)

## Usage Examples:

### In Razor components:
```html
<img src="~/images/logo.svg" alt="App Logo" class="navbar-brand-img" />
<img src="~/images/icons/vehicle.svg" alt="Vehicle" class="feature-icon" />
```

### In CSS:
```css
.app-logo {
    background-image: url('/images/logo.svg');
}
```

### Recommended image formats:
- **Icons**: SVG (scalable, small file size)
- **Logos**: SVG or PNG with transparency
- **Photos**: JPEG for photos, PNG for graphics with transparency
- **Favicons**: ICO, PNG (multiple sizes)
